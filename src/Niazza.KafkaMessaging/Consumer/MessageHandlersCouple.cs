using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Niazza.KafkaMessaging.ErrorHandling;
using Niazza.KafkaMessaging.Serializers;

namespace Niazza.KafkaMessaging.Consumer
{
    public class MessageHandlersCouple
    {
        public MessageHandlersCouple(
            Type messageType, 
            IEnumerable<Type> handlerTypes, 
            IDictionary<ExecutionResult, ErrorHandlingConfiguration>  errorHandlingConfigurations, 
            MainConsumerBehavior? behavior = null, 
            int? intervalInMs = null,
            IMessageSerialization serializer = null)
        {
            MessageType = messageType;
            HandlerTypes = new ConcurrentBag<Type>(handlerTypes);
            ErrorHandlingConfigurations = errorHandlingConfigurations;
            Behavior = behavior;
            IntervalInMs = intervalInMs;
            _serializer = serializer ?? new JsonMessageSerialization();
        }

        private IMessageSerialization _serializer;

        public Type MessageType { get; }
        
        public ConcurrentBag<Type> HandlerTypes { get; }
        
        public IDictionary<ExecutionResult, ErrorHandlingConfiguration> ErrorHandlingConfigurations { get; }
        /// <summary>
        /// If set - defines the consumer to handle topic
        /// </summary>
        public MainConsumerBehavior? Behavior { get; }
        /// <summary>
        /// 0 - default. The interval between topic messages
        /// </summary>
        public int? IntervalInMs { get; }

        public object Deserialize(string  message) => _serializer.Deserialize(message, MessageType);
        
    }
}