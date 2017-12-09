﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Remiworks.Core;
using Remiworks.Core.Models;

namespace Remiworks.Attributes.Initialization
{
    public class Initializer
    {
        private ILogger Logger { get; } = RemiLogging.CreateLogger<Initializer>();
        private readonly IBusProvider _busProvider;
        private readonly IServiceProvider _serviceProvider;

        public Initializer(IBusProvider busProvider, IServiceProvider serviceProvider)
        {
            _busProvider = busProvider;
            _serviceProvider = serviceProvider;
        }

        public void Initialize(Assembly executingAssembly)
        {
            Logger.LogInformation("Initializing event bus");
            _busProvider.CreateConnection();

            var types = executingAssembly.GetTypes();
            InitializeQueueListeners(types);
            Logger.LogInformation($"Initialization completed. Now listening...");
        }

        private void InitializeQueueListeners(IEnumerable<Type> types)
        {
            Logger.LogInformation("Initializing event listeners");
            foreach (var type in types)
            {
                var queueAttribute = type.GetCustomAttribute<QueueListenerAttribute>();

                if (queueAttribute != null)
                {
                    if (TypeContainsBothCommandsAndEvents(type))
                    {
                        throw new InvalidOperationException("Type {} can't contain both events and commands. Events and commands should not be sent to the same queue.");
                    }

                    var methods = type.GetMethods();

                    if (methods.Any(m => m.GetCustomAttributes<EventAttribute>().Any()))
                    {
                        SetUpTopicMethods(type, queueAttribute.QueueName);
                        Logger.LogInformation($"Initializing event {type}");
                    }
                    else if (methods.Any(m => m.GetCustomAttributes<CommandAttribute>().Any()))
                    {
                        SetUpCommandMethods(type, queueAttribute.QueueName);
                        Logger.LogInformation($"Initializing commands {type}");
                    }
                }
            }
        }

        private bool TypeContainsBothCommandsAndEvents(Type type)
        {
            var methods = type.GetMethods();

            return
                methods.Any(m => m.GetCustomAttributes<EventAttribute>().Any()) &&
                methods.Any(m => m.GetCustomAttributes<CommandAttribute>().Any());
        }

        private void SetUpCommandMethods(Type type, string queueName)
        {
            Dictionary<string, MethodInfo> commandsWithMethods = GetCommandsWithMethods(type);
            _busProvider.SetupRpcListeners(queueName, commandsWithMethods.Keys.ToArray(), CreateCommandReceivedCallback(type, commandsWithMethods));
        }

        private void SetUpTopicMethods(Type type, string queueName)
        {
            Dictionary<string, MethodInfo> topicsWithMethods = GetTopicsWithMethods(type);
            _busProvider.CreateTopicsForQueue(queueName, topicsWithMethods.Keys.ToArray());
            var callback = CreateEventReceivedCallback(type, topicsWithMethods);
            _busProvider.BasicConsume(queueName, callback);
        }

        public Dictionary<string, MethodInfo> GetCommandsWithMethods(Type type)
        {
            return GetAttributeValuesWithMethod<CommandAttribute>(type, (a) => a.Key);
        }

        public Dictionary<string, MethodInfo> GetTopicsWithMethods(Type type)
        {
            return GetAttributeValuesWithMethod<EventAttribute>(type, (a) => a.Topic);
        }

        private Dictionary<string, MethodInfo> GetAttributeValuesWithMethod<TAttribute>(Type type, Func<TAttribute, string> predicate) where TAttribute : Attribute
        {
            return type.GetMethods()
                .Where(m => m.GetCustomAttribute<TAttribute>() != null)
                .ToDictionary(m => predicate(m.GetCustomAttribute<TAttribute>()), m => m);
        }

        public EventReceivedCallback CreateEventReceivedCallback(Type type, Dictionary<string, MethodInfo> topics)
        {
            return (message) =>
            {
                var instance = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, type);

                var topicMatches = GetTopicMatches(message.RoutingKey, topics);

                foreach (var topic in topicMatches)
                {
                    InvokeTopic(message, instance, topic);
                }
            };
        }

        public CommandReceivedCallback CreateCommandReceivedCallback(Type type, Dictionary<string, MethodInfo> commands)
        {
            return async (message) =>
            {
                var instance = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, type);

                (string name, MethodInfo method) = GetCommandMatch(message.RoutingKey, commands);

                var result = await InvokeCommand(message, name, instance, method);

                return result;
            };
        }

        private async Task<string> InvokeCommand(EventMessage message, string name, object instance, MethodInfo method)
        {
            try
            {
                Logger.LogInformation($"Command {name} has been invoked", message);
                object[] parameters = ConstructMethodParameters(message.JsonMessage, method);

                object result = null;

                if (method.ReturnType.BaseType != typeof(Task))
                {
                    result = await Task.Run(() => method.Invoke(instance, parameters));
                }
                else
                {
                    result = await (dynamic)method.Invoke(instance, parameters);
                }

                return JsonConvert.SerializeObject(result);
            }
            catch (TargetInvocationException ex)
            {
                Logger.LogWarning(ex.InnerException, "Exception was thrown for a command", new object[] { instance.ToString(), name, method });
                
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.InnerException, $"Exception was thrown for the command {name}", new object[] { instance.ToString(), name, method });
                
                throw;
            }
        }

        private void InvokeTopic(EventMessage message, object instance, KeyValuePair<string, MethodInfo> topic)
        {
            try
            {
                Logger.LogInformation($"Topic {topic.Key} has been invoked", message);
                var parameters = topic.Value.GetParameters();
                var parameter = parameters.FirstOrDefault();
                var paramType = parameter.ParameterType;
                var arguments = ConstructMethodParameters(message.JsonMessage, topic.Value);

                topic.Value.Invoke(instance, arguments);
            }
            catch (TargetInvocationException ex)
            {
                Logger.LogError(ex.InnerException, "Exception was thrown for a topic", new object[] { instance.ToString(), topic.Key, topic.Value });
            }
        }

        private static (string, MethodInfo) GetCommandMatch(string routingKey, Dictionary<string, MethodInfo> commands)
        {
            var command = commands.SingleOrDefault(c => c.Key == routingKey);

            return (command.Key, command.Value);
        }

        public static Dictionary<string, MethodInfo> GetTopicMatches(string routingKey, Dictionary<string, MethodInfo> topics)
        {
            var regexHashTag = @"\w+(\.\w+)*";
            var regexStar = @"[\w]+";
            var topicMatches = new Dictionary<string, MethodInfo>();

            foreach (var topic in topics)
            {
                var pattern = topic.Key
                    .Replace(".", "\\.")
                    .Replace("*", regexStar)
                    .Replace("#", regexHashTag);

                pattern = $"^{pattern}$";

                if (Regex.IsMatch(routingKey, pattern))
                {
                    topicMatches.Add(topic.Key, topic.Value);
                }
            }

            return topicMatches;
        }

        private static object[] ConstructMethodParameters(string message, MethodBase method)
        {
            var parameters = method.GetParameters();
            var parameter = parameters.FirstOrDefault();

            if (parameter != null)
            {
                var paramType = parameter.ParameterType;
                var arguments = JsonConvert.DeserializeObject(message, paramType);

                return new object[] {arguments};
            }

            return null;
        }
    }
}
