using System;
using System.Threading;
using System.Threading.Tasks;
using Niazza.KafkaMessaging.Serializers;

namespace Niazza.KafkaMessaging.Producer
{
    public interface IAsyncProducer: IDisposable
    {
        Task ProduceAsync<TMessage>(TMessage message, CancellationToken token = default(CancellationToken),
            string topic = null, IMessageSerialization serialization = null)
            where TMessage : class;
    }
}