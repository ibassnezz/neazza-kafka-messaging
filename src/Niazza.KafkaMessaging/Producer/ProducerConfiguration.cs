namespace Niazza.KafkaMessaging.Producer
{
    public class ProducerConfiguration
    {
        /// <summary>
        /// Comma separated bootstrap servers for kafka 
        /// </summary>
        public string Servers { get; set; }

        public AckToBrokersType AckType { get; set; }

        public MessageIdempotency Idempotency { get; set; }
    }
}