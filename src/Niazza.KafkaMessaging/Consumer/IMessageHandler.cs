using System.Threading;
using System.Threading.Tasks;

namespace Niazza.KafkaMessaging.Consumer
{
    public interface IMessageHandler
    {
        Task<ExecutionResult> HandleAsync(object message, CancellationToken cancellationToken);
    }
}