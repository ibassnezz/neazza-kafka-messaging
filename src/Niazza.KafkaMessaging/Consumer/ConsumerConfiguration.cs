namespace Niazza.KafkaMessaging.Consumer
{
    public class ConsumerConfiguration
    {
        /// <summary>
        /// Comma separated bootstrap servers for kafka 
        /// </summary>
        public string Servers { get; set; }
        
        /// <summary>
        /// Only one consumer from the group can catch the message 
        /// </summary>
        public string GroupId { get; set; }
        
        /// <summary>
        /// False  - Do NOT commit on ErrorHandling when error or exception appears and ROLLBACKs the caret on the partition. Does not use handler Error Strategies
        /// True - Depends on *Behavior* property if *Auto* - then autocomits if *Hybrid* - it commits after all handlers are executed
        /// </summary>
        public bool IsAutocommitErrorHandling { get; set; }
        
        /// <summary>
        /// Cooldown interval
        /// Actual only if IsAutocommitErrorHandling == false
        /// Default 1000ms
        /// </summary>
        public int ManualCommitIntervalMs { get; set; }
        
        /// <summary>
        /// Choices: AutoCommit, Hybrid
        /// </summary>
        public MainConsumerBehavior Behavior { get; set; }

        /// <summary>
        /// Actual only if IsAutocommitErrorHandling == true
        /// the interval between sequentially asynchronous handlers
        /// Default is 0
        /// </summary>
        public int AsyncHandlingIntervalMs { get; set; }

        /// <summary>
        /// The maximum length of time (in milliseconds) before a cancellation request is acted on.
        /// Low values may result in measurably higher CPU usage.
        /// default: 100 range: 1 <= dotnet.cancellation.delay.max.ms <= 10000 importance: low 
        /// </summary>
        internal int CancellationDelayMaxMs { get; set; }

        public NoOffsetType NoOffsetType { get; set; }

        /// <summary>
        /// Gets interval between sequentially asynchronous handlers
        /// </summary>
        /// <param name="overridableInterval">If null then AsyncHandlingIntervalMs</param>
        /// <returns></returns>
        public int GetAsyncHandlingIntervalMs(int? overridableInterval)
        {
            return overridableInterval ?? AsyncHandlingIntervalMs;
        }

        public string ErrorTopicPrefix { get; set; }
        
    }

    public enum NoOffsetType
    {
        TakeEarliest,
        TakeLatest
    }
}