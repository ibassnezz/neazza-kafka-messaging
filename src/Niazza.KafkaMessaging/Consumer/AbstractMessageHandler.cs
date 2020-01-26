using System.Threading;
using System.Threading.Tasks;

namespace Niazza.KafkaMessaging.Consumer
{
    public abstract class AbstractMessageHandler<TMessage>: IMessageHandler where TMessage: class
    {
        protected abstract Task<ExecutionResult> HandleAsync(TMessage message, CancellationToken cancellationToken);
        
        public Task<ExecutionResult> HandleAsync(object message, CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested
                ? Task.FromResult(ExecutionResult.Cancelled)
                : HandleAsync((TMessage) message, cancellationToken);
        }
    }
}