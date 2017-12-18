﻿using System;
using System.Threading.Tasks;

namespace Remiworks.Core.Command.Listener
{
    public interface ICommandListener
    {
        Task SetupCommandListenerAsync<TParam>(string queueName, string key, CommandReceivedCallback<TParam> callback);

        Task SetupCommandListenerAsync(string queueName, string key, CommandReceivedCallback callback, Type parameterType);
    }
    
    public delegate object CommandReceivedCallback<in TParam>(TParam parameter);

    public delegate object CommandReceivedCallback(object parameter);
}