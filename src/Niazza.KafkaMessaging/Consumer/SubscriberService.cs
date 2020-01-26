using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Niazza.KafkaMessaging.ErrorHandling;
using Niazza.KafkaMessaging.Exceptions;
using Niazza.KafkaMessaging.Serializers;

namespace Niazza.KafkaMessaging.Consumer
{
    internal class SubscriberService : ISubscriberService
    {
        
        private readonly ConcurrentDictionary<string, MessageHandlersCouple> _subscribers = new ConcurrentDictionary<string, MessageHandlersCouple>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorHandlingConfigurationBinding">Sets strategy of the error handling</param>
        /// <param name="topic">custom topic</param>
        /// <param name="postfix">set postfix to topic if it has a value</param>
        /// <param name="behavior">customizes commit behavior</param>
        /// <param name="asyncHandlingIntervalMs">Sets interval between asynchronous messages. Default are used from common parameter. Valid only in AutoCommit</param>
        /// <typeparam name="THandler">Handler of the message</typeparam>
        /// <typeparam name="TMessage">Massage for topic. Covered with KafkaMessageAttribute</typeparam>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="SubscribeException">No attribute KafkaMessageAttribute is found and topic name has not been set</exception>
        public void Subscribe<THandler, TMessage>(
                IDictionary<ExecutionResult, ErrorHandlingConfiguration> errorHandlingConfigurationBinding = null, 
                string topic = null,
                MainConsumerBehavior? behavior = null,
                int? asyncHandlingIntervalMs = null)
            where TMessage : class
            where THandler: AbstractMessageHandler<TMessage>
         => Subscribe(typeof(THandler), typeof(TMessage), errorHandlingConfigurationBinding, topic);


        internal void Subscribe(
            Type handlerType, 
            Type messageType, 
            IDictionary<ExecutionResult, ErrorHandlingConfiguration> errorHandlingConfigurationBinding = null, 
            string topic = null,  
            MainConsumerBehavior? behavior = null,
            int? asyncHandlingIntervalMs = null,
            IMessageSerialization messageSerialization = null)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                if (!(messageType.GetCustomAttribute(typeof(KafkaMessageAttribute)) is KafkaMessageAttribute kafkaAttribute))
                {
                    throw new SubscribeException($"The message has no {nameof(KafkaMessageAttribute)}");
                }

                topic = kafkaAttribute.Topic;
            }

            if (_subscribers.TryGetValue(topic, out var couple))
            {
                if (couple.MessageType == messageType)
                {
                    couple.HandlerTypes.Add(handlerType);
                }
            }
            else
            {
                if (!_subscribers.TryAdd(topic, 
                    new MessageHandlersCouple(
                        messageType, 
                        new[] {handlerType},
                        BindConfigurations(errorHandlingConfigurationBinding), 
                        behavior, 
                        asyncHandlingIntervalMs,
                        messageSerialization)))
                {
                    throw new ArgumentException($"Topic Key {topic} already exists");
                };
            }
        }

        private static IDictionary<ExecutionResult, ErrorHandlingConfiguration> BindConfigurations(IDictionary<ExecutionResult, ErrorHandlingConfiguration> configurations = null)
        {
            var defaultConfiguration = new Dictionary<ExecutionResult, ErrorHandlingConfiguration>
            {
                {ExecutionResult.Cancelled, ErrorHandlingConfiguration.GetRepeatOnce()},
                {ExecutionResult.Failed, ErrorHandlingConfiguration.GetScheduled()},
                {ExecutionResult.PassOut, ErrorHandlingConfiguration.GetRepeatOnce()},
                {ExecutionResult.FailFinalized, ErrorHandlingConfiguration.GetNone()}
            };

            if (configurations == null) return defaultConfiguration;
            
            foreach (var errorHandlingConfiguration in configurations)
            {
                defaultConfiguration[errorHandlingConfiguration.Key] = errorHandlingConfiguration.Value;
            }
            return defaultConfiguration;
        }

        public MessageHandlersCouple GetMessageHandlersCouple(string topicName)
        {
            return string.IsNullOrEmpty(topicName)? null: _subscribers.ContainsKey(topicName)?_subscribers[topicName]: null ;
        }

        public IEnumerable<string> GetTopicList(string except)
        {
            return _subscribers.Keys.Where(topic => string.IsNullOrEmpty(except) || !topic.StartsWith(except));
        }
    }
}