﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitFramework.Contracts;
using RabbitFramework.Models;
using Shouldly;
using System;
using System.Threading.Tasks;

namespace RabbitFramework.Test
{
    [TestClass]
    public class RabbitBusProviderTests
    {
        private const string TopicsParamName = "topics";
        private const string CallbackParamName = "callback";
        private const string QueueNameParamName = "queueName";
        private const string EventMessageParamName = "eventMessage";
        private const string FunctionParamName = "function";
        private const string MessageParamName = "message";

        private readonly string[] _topics = new string[] { "SomeTopic1", "SomeTopic2" };

        private readonly Mock<BusOptions> _busOptionsMock = new Mock<BusOptions>();
        private readonly CommandReceivedCallback _commandReceivedCallback = (p) => { return Task.FromResult("callback"); };

        private RabbitBusProvider _sut;

        [TestInitialize]
        public void Initialize()
        {
            _sut = new RabbitBusProvider(_busOptionsMock.Object);
        }

        [TestMethod]
        public void ConstructorSetsBusOptions()
        {
            _sut.BusOptions.ShouldBe(_busOptionsMock.Object);
        }

        [TestMethod]
        public void BasicConsumeThrowsArgumentExceptionWhenQueueNameIsNull()
        {
            EventReceivedCallback callback = new EventReceivedCallback((message) => { });

            var exception = Should.Throw<ArgumentNullException>(() => _sut.BasicConsume(null, callback));
            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void BasicConsumeThrowsArgumentExceptionWhenQueueNameIsEmpty()
        {
            EventReceivedCallback callback = new EventReceivedCallback((message) => { });

            var exception = Should.Throw<ArgumentNullException>(() => _sut.BasicConsume("", callback));
            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void BasicConsumeThrowsArgumentExceptionWhenQueueNameIsWhiteSpace()
        {
            EventReceivedCallback callback = new EventReceivedCallback((message) => { });

            var exception = Should.Throw<ArgumentNullException>(() => _sut.BasicConsume(" ", callback));
            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void BasicConsumeThrowsArgumentExceptionWhenCallbackIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => _sut.BasicConsume("SomeQueue", null));
            exception.ParamName.ShouldBe(CallbackParamName);
        }

        [TestMethod]
        public void CreateQueueWithTopicsThrowsArgumentExceptionWhenQueueNameIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => _sut.CreateTopicsForQueue(null, _topics));
            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void CreateQueueWithTopicsThrowsArgumentExceptionWhenQueueNameIsEmpty()
        {
            var exception = Should.Throw<ArgumentNullException>(() => _sut.CreateTopicsForQueue("", _topics));
            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void CreateQueueWithTopicsThrowsArgumentExceptionWhenQueueNameIsWhiteSpace()
        {
            var exception = Should.Throw<ArgumentNullException>(() => _sut.CreateTopicsForQueue(" ", _topics));
            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void CreateQueueWithTopicsThrowsArgumentExceptionWhenTopicsIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => _sut.CreateTopicsForQueue("SomeQueue", null));
            exception.ParamName.ShouldBe(TopicsParamName);
        }

        [TestMethod]
        public void CreateQueueWithTopicsThrowsArgumentExceptionWhenTopicsContainsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => _sut.CreateTopicsForQueue("SomeQueue", new string[] { null, "someTopic" }));
            exception.ParamName.ShouldBe(TopicsParamName);
        }

        [TestMethod]
        public void CreateQueueWithTopicsThrowsArgumentExceptionWhenTopicsIsEmpty()
        {
            var exception = Should.Throw<ArgumentNullException>(() => _sut.CreateTopicsForQueue("SomeQueue", new string[] { }));
            exception.ParamName.ShouldBe(TopicsParamName);
        }

        [TestMethod]
        public void BasicPublishThrowsArgumentExceptionWhenMessageIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => _sut.BasicPublish(null));
            exception.ParamName.ShouldBe(EventMessageParamName);
        }

        [TestMethod]
        public void SetupRpcListenerThrowsArgumentExceptionWhenQueueNameIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => _sut.SetupRpcListeners(null, null, _commandReceivedCallback));
            exception.ParamName.ShouldBe("The queueName should not be null");
        }

        [TestMethod]
        public void SetupRpcListenerThrowsArgumentExceptionWhenQueueNameIsEmpty()
        {
            var exception = Should.Throw<ArgumentNullException>(() => _sut.SetupRpcListeners("", null,_commandReceivedCallback));
            exception.ParamName.ShouldBe("The queueName should not be null");
        }

        [TestMethod]
        public void SetupRpcListenerThrowsArgumentExceptionWhenQueueNameIsWhitespace()
        {
            var exception = Should.Throw<ArgumentNullException>(() => _sut.SetupRpcListeners(" ", null,_commandReceivedCallback));
            exception.ParamName.ShouldBe("The queueName should not be null");
        }

        [TestMethod]
        public void SetupRpcListenerThrowsArgumentExceptionWhenFuntionIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => _sut.SetupRpcListeners("SomeQueue", null, null));
            exception.ParamName.ShouldBe("The function should not be null");
        }
    }
}