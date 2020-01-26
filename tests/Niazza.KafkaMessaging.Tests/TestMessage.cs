namespace Niazza.KafkaMessaging.Tests
{
    [KafkaMessage("test_message")]
    public class TestMessage
    {
        public string Number { get; set; }
    }
    
    public class NoAttributeTestMessage
    {
        public string Number { get; set; }
    }
}