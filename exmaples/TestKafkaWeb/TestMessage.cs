using Niazza.KafkaMessaging;

namespace TestKafkaWeb
{
    [KafkaMessage("test_autocommit")]
    public class Notify
    {
        public string Message { get; set; }
    }
}