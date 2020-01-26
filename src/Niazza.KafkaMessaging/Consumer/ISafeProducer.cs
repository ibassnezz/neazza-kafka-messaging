using System.Threading.Tasks;

namespace Niazza.KafkaMessaging.Consumer
{
    internal interface ISafeProducer
    {
        Task ProduceSafeAsync<TMessage>(TMessage message, string topic) where TMessage: class;
    }
}