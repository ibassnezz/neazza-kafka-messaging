using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Niazza.KafkaMessaging.Consumer
{
    internal abstract class AbstractCommitConsumingBehavior : IConsumingBehavior
    {
        protected readonly ILogger Logger;
        protected readonly ISubscriberService SubscriberService;

        protected AbstractCommitConsumingBehavior(ILogger logger, ISubscriberService subscriberService)
        {
            Logger = logger;
            SubscriberService = subscriberService;
        }

        [HandleProcessCorruptedStateExceptions]
        public async Task RunConsumePollingAsync(IConsumer<Ignore, string> consumer, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await ConsumeAsync(consumer, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    consumer.Unsubscribe();
                    Logger.LogInformation("Polling task has been cancelled");
                }
                catch (ConsumeException e)
                {
                    Logger.LogError("Error while consuming messages", e);
                    await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
                }
                catch (Exception e)
                {
                    if (e.InnerException is AccessViolationException) return;
                    Logger.LogError(e, "Unexpected exception was raised");
                    await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
                }
            }
        }

        protected abstract Task ConsumeAsync(IConsumer<Ignore, string> consumer, CancellationToken cancellationToken);
    }
}