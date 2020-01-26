using System.Collections.Generic;
using Niazza.KafkaMessaging.Consumer;

namespace Niazza.KafkaMessaging.ErrorHandling
{
    internal class MessagingBus: IMessagingBus
    {
        private readonly ISubscriberService _subscriberService;

        public MessagingBus(ISubscriberService subscriberService)
        {
            _subscriberService = subscriberService;
        }

        public void Subscribe<THandler, TMessage>(IDictionary<ExecutionResult, ErrorHandlingConfiguration> errorHandlingConfigurationBinding = null, string topic = null) 
            where THandler : AbstractMessageHandler<TMessage> where TMessage : class
        {
            _subscriberService.Subscribe<THandler, TMessage>();
        }
    }
}