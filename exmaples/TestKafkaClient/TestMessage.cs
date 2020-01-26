using Niazza.KafkaMessaging;

namespace TestKafkaClient
{
    public class Notify
    {
        public string Message { get; set; }
    }
    
    [KafkaMessage("test_autocommit")]
    public class UnderNotify : Notify
    {
        
    }
}