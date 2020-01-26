using System;
using Niazza.KafkaMessaging.Serializers;

namespace Niazza.KafkaMessaging.Producer
{
    public interface IProducer: IDisposable
    {
        void BeginProduce<TMessage>(TMessage message, string topic = null, bool ignorePostfix = false, IMessageSerialization serialization = null) where TMessage: class;
    }
}