﻿using System;
using System.Runtime.Serialization;

namespace Remiworks.Core.Exceptions
{
    [Serializable]
    public class CommandPublisherException : Exception
    {
        public CommandPublisherException()
        {
        }

        public CommandPublisherException(string message) : base(message)
        {
        }

        public CommandPublisherException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CommandPublisherException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}