using System.Collections.Generic;
using Niazza.KafkaMessaging.ErrorHandling;
using Niazza.KafkaMessaging.Serializers;

namespace Niazza.KafkaMessaging.Consumer
{
    public class CustomizedHandlerSettings
    {
        public CustomizedHandlerSettings(
            IDictionary<ExecutionResult, ErrorHandlingConfiguration> errorHandlerConfiguration = null,
            string topic = null,
            MainConsumerBehavior? behavior = null, 
            int? asyncHandlingIntervalMs = null,
            IMessageSerialization serialization = null)
        {
            Serialization = serialization;
            Topic = topic;
            ErrorHandlerConfiguration = errorHandlerConfiguration ?? new Dictionary<ExecutionResult, ErrorHandlingConfiguration>();
            Behavior = behavior;
            AsyncHandlingIntervalMs = asyncHandlingIntervalMs;
        }

        /// <summary>
        /// A customized serializer
        /// Json is default
        /// </summary>
        public IMessageSerialization Serialization { get; }

        /// <summary>
        /// Customized topic
        /// </summary>
        public string Topic { get; }
        
        /// <summary>
        /// customizes commit behavior. if null - takes common
        /// </summary>
        
        public IDictionary<ExecutionResult, ErrorHandlingConfiguration> ErrorHandlerConfiguration { get; }

        /// <summary>
        /// Set custom behavior for topic
        /// </summary>
        public MainConsumerBehavior? Behavior { get; }
        /// <summary>
        /// interval between asynchronous messages. Default (null) are used from common parameter. Valid only in AutoCommit
        /// </summary>
        public int? AsyncHandlingIntervalMs { get; }
    }
}
