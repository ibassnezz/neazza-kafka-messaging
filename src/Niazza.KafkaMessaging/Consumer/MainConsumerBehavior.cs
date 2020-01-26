namespace Niazza.KafkaMessaging.Consumer
{
    public enum MainConsumerBehavior
    {
        /// <summary>
        /// Commits when message is received. Error handling depends on IsAutocommitErrorHandling
        /// </summary>
        AutoCommit,
        
        /// <summary>
        /// Commits when all selected handlers was executed.
        /// Errors will be handled using strategies
        /// If the Hybrid was selected and IsAutocommitErrorHandling == true
        /// then error strategies will be ignored - the rollback will be used
        /// </summary>
        Hybrid,

        /// <summary>
        /// Commits when Message was handled correctly
        /// Error strategies is FULLY ignored.
        /// Warn, a huge message lags could happen until exception won't be catch or disappeared
        /// </summary>
        Manual
    }
}