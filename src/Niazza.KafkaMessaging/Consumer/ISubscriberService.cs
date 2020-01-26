using System;
using System.Collections.Generic;
using Niazza.KafkaMessaging.ErrorHandling;

namespace Niazza.KafkaMessaging.Consumer
{
    internal interface ISubscriberService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorHandlingConfigurationBinding">Sets strategy of the error handling</param>
        /// <param name="topic">custom topic</param>
        /// <param name="behavior">Set custom behavior for topic</param>
        /// <param name="asyncHandlingIntervalMs">Sets interval between asynchronous messages. Default are used from common parameter. Valid only in AutoCommit</param>
        /// <typeparam name="THandler">Handler of the message</typeparam>
        /// <typeparam name="TMessage">Massage for topic</typeparam>
        /// <exception cref="ArgumentException"></exception>
        void Subscribe<THandler, TMessage>(IDictionary<ExecutionResult, ErrorHandlingConfiguration> errorHandlingConfigurationBinding = null, 
                string topic = null, 
                MainConsumerBehavior? behavior = null,
                int? asyncHandlingIntervalMs = null) 
            where TMessage : class 
            where THandler: AbstractMessageHandler<TMessage>;

        MessageHandlersCouple GetMessageHandlersCouple(string topicName);
        /// <summary>
        /// Get all registered topics
        /// </summary>
        /// <param name="except">if except is null or empty then get all topics</param>
        /// <returns></returns>
        IEnumerable<string> GetTopicList(string except);
    }
}