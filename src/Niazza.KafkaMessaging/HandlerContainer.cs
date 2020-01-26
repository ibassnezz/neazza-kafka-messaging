using System;
using System.Collections.Generic;
using Niazza.KafkaMessaging.Consumer;
using Niazza.KafkaMessaging.ErrorHandling;

namespace Niazza.KafkaMessaging
{
    public class HandlerContainer<THandler, TMessage> : IHandlerContainer where TMessage : class
        where THandler : AbstractMessageHandler<TMessage>
    {

        public HandlerContainer(
            Func<IServiceProvider, object> handlerFunc,
            IDictionary<ExecutionResult, ErrorHandlingConfiguration> errorHandlerConfiguration = null,
            string topic = null) : this(new CustomizedHandlerSettings(errorHandlerConfiguration, topic))
        {
            HandlerFunc = handlerFunc;
        }

        public HandlerContainer(
            IDictionary<ExecutionResult, ErrorHandlingConfiguration> errorHandlerConfiguration = null,
            string topic = null) : this(new CustomizedHandlerSettings(errorHandlerConfiguration, topic)) { }

        public HandlerContainer(CustomizedHandlerSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// for all ExecutionResult.Failed, ErrorHandlingConfiguration.GetScheduled()
        /// </summary>
        public HandlerContainer() :this(new CustomizedHandlerSettings()) { }

        public Type HandlerType => typeof(THandler);
        public Type MessageType => typeof(TMessage);
        public Func<IServiceProvider, object> HandlerFunc { get; }
        public CustomizedHandlerSettings Settings { get; }
    }
}