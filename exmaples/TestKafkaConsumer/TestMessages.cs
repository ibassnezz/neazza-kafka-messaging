using System.Xml.Serialization;
using Niazza.KafkaMessaging;

namespace TestKafkaConsumer
{
    [KafkaMessage("test_autocommit")]
    public class NotifyAutoCommit
    {
        public string Message { get; set; }
    }

    [KafkaMessage("test_hybrid")]
    public class NotifyHybrid
    {
        public string Message { get; set; }
    }

    [KafkaMessage("test_manualcommit")]
    public class NotifyManualCommit
    {
        public string Message { get; set; }
    }


    [KafkaMessage("test_xmlmessage")]
    public class NotifyXmlMessage
    {
        [XmlElement(ElementName =  "message")]
        public string Message { get; set; }
    }
}