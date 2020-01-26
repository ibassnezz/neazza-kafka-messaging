using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Niazza.KafkaMessaging.Consumer
{
    internal interface IConsumingBehavior
    {
        Task RunConsumePollingAsync(IConsumer<Ignore, string> consumer, CancellationToken cancellationToken);
    }
}