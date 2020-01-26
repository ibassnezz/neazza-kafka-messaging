using System;

namespace Niazza.KafkaMessaging.Serializers
{
    public interface IMessageSerialization
    {
        object Deserialize(string message, Type type);

        string Serialize(object objectToSerialize);
    }
}
