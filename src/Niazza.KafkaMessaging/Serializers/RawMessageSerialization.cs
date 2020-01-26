using System;

namespace Niazza.KafkaMessaging.Serializers
{
    public class RawMessageSerialization : IMessageSerialization
    {
        public object Deserialize(string message, Type type) => message;
        
        public string Serialize(object objectToSerialize) => objectToSerialize?.ToString();
    }
}
