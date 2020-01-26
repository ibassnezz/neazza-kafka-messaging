namespace Niazza.KafkaMessaging.Consumer
{
    public enum ExecutionResult
    {
        Acknowledged,
        PassOut,
        Cancelled,
        Failed,
        /// <summary>
        /// Message at least failed and will be sent
        /// to external provider
        /// </summary>
        FailFinalized
    }
}