namespace Niazza.KafkaMessaging.Producer
{
    public enum MessageIdempotency
    {
        /// <summary>
        /// Default is non idempotent
        /// </summary>
        UseDefault = 0,
        /// <summary>
        /// makes all messages idempotent
        /// overrides AckType configurations
        /// Ack sets toAll automatically
        /// </summary>
        UseIdempotent
    }
}