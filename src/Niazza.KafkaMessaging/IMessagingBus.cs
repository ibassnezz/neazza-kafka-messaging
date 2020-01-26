using System.Collections.Generic;
using Niazza.KafkaMessaging.Consumer;
using Niazza.KafkaMessaging.ErrorHandling;

namespace Niazza.KafkaMessaging
{
    public interface IMessagingBus
    {
        void Subscribe<THandler, TMessage>(
            IDictionary<ExecutionResult, ErrorHandlingConfiguration> errorHandlingConfigurationBinding = null,
            string topic = null)
            where TMessage : class 
            where THandler: AbstractMessageHandler<TMessage>;
    }
}