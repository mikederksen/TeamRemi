﻿using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Remiworks.Core;
using Remiworks.Core.Models;
using Shouldly;

namespace Remiworks.RabbitMQ.Test
{
    [TestClass]
    public class RabbitBusProviderParameterTests
    {
        private const string TopicsParamName = "topic";
        private const string CallbackParamName = "callback";
        private const string QueueNameParamName = "queueName";
        private const string EventMessageParamName = "eventMessage";
        private const string JsonMessageParamName = "eventMessage.JsonMessage";
        private const string BusOptionsParamName = "busOptions";

        private const string Topic = "SomeTopic1";

        private readonly Mock<BusOptions> _busOptionsMock = new Mock<BusOptions>();
        private readonly CommandReceivedCallbackS _commandReceivedCallback = (p) => { return Task.FromResult("callback"); };

        private RabbitBusProvider _sut;

        [TestInitialize]
        public void Initialize()
        {
            _sut = new RabbitBusProvider(_busOptionsMock.Object);
        }

        [TestMethod]
        public void ConstructorThrows_ArgumentNullException_WhenBusOptionsIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => new RabbitBusProvider(null));
            exception.ParamName.ShouldBe(BusOptionsParamName);
        }

        [TestMethod]
        public void ConstructorSetsBusOptions()
        {
            _sut.BusOptions.ShouldBe(_busOptionsMock.Object);
        }

        [TestMethod]
        public void BasicConsumeThrows_ArgumentNullException_WhenQueueNameIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => 
                _sut.BasicConsume(null, new Mock<EventReceivedCallback>().Object));

            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void BasicConsumeThrows_ArgumentException_WhenQueueNameIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() => 
                _sut.BasicConsume("", new Mock<EventReceivedCallback>().Object));

            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void BasicConsumeThrows_ArgumentException_WhenQueueNameIsWhiteSpace()
        {
            var exception = Should.Throw<ArgumentException>(() => 
                _sut.BasicConsume(" ", new Mock<EventReceivedCallback>().Object));

            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void BasicConsumeThrows_ArgumentNullException_WhenCallbackIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => 
                _sut.BasicConsume("SomeQueue", null));

            exception.ParamName.ShouldBe(CallbackParamName);
        }

        [TestMethod]
        public void CreateQueueWithTopicsThrows_ArgumentNullException_WhenQueueNameIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => 
                _sut.BasicTopicBind(null, Topic));

            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void CreateQueueWithTopicsThrows_ArgumentException_WhenQueueNameIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() => 
                _sut.BasicTopicBind("", Topic));

            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void CreateQueueWithTopicsThrows_ArgumentException_WhenQueueNameIsWhiteSpace()
        {
            var exception = Should.Throw<ArgumentException>(() => 
                _sut.BasicTopicBind(" ", Topic));

            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void CreateQueueWithTopicsThrows_ArgumentNullException_WhenTopicsIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => 
                _sut.BasicTopicBind("SomeQueue", null));

            exception.ParamName.ShouldBe(TopicsParamName);
        }

        [TestMethod]
        public void BasicPublishThrows_ArgumentNullException_WhenMessageIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.BasicPublish(null));

            exception.ParamName.ShouldBe(EventMessageParamName);
        }

        [TestMethod]
        public void BasicPublishThrows_ArgumentNullException_WhenJsonMessage_IsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => 
                _sut.BasicPublish(new EventMessage()));

            exception.ParamName.ShouldBe(JsonMessageParamName);
        }
    }
}