using System;

namespace Niazza.KafkaMessaging
{
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class KafkaMessageAttribute: Attribute
    {
        public string Topic { get; }
        
        public KafkaMessageAttribute(string topic)
        {
            Topic = topic;
        }
    }
}