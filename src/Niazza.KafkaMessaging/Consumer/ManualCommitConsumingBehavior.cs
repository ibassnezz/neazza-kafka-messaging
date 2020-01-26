using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Niazza.KafkaMessaging.ErrorHandling;

namespace Niazza.KafkaMessaging.Consumer
{
    internal class ManualCommitConsumingBehavior: AbstractCommitConsumingBehavior
    {
        private readonly IErrorSaver _errorSaver;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _coolDownInterval;
        private static readonly ExecutionResult[] AllowedToCommitStatuses = 
            {ExecutionResult.Acknowledged, ExecutionResult.FailFinalized, ExecutionResult.PassOut};


        public ManualCommitConsumingBehavior(ILogger<ManualCommitConsumingBehavior> logger, 
            ISubscriberService subscriberService, IErrorSaver errorSaver, 
            ConsumerConfiguration consumerConfiguration, IServiceProvider serviceProvider) : base(logger, subscriberService)
        {
            _errorSaver = errorSaver;
            _serviceProvider = serviceProvider;
            _coolDownInterval = consumerConfiguration.ManualCommitIntervalMs;
        }

        /// <summary>
        /// Pay attention on your handler set. If any of handlers isn't "Ack", the message won't be committed
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="HandlerNotFoundException"></exception>
        protected override async Task ConsumeAsync(IConsumer<Ignore, string> consumer, CancellationToken cancellationToken)
        {
            var message = consumer.Consume(cancellationToken);
            
            Logger.LogInformation("New message received {topic} {message}", message.Topic, message.Value);
            var couple = SubscriberService.GetMessageHandlersCouple(message.Topic);
            if (couple == null)
            {
                await _errorSaver.SaveMassageAsync(message);
                //there is no chance to recover this message
                consumer.Commit(message);
                return;
            }

            object deserializedData = null;
            try
            {
                deserializedData = couple.Deserialize(message.Value);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Cannot parse message");
                consumer.Commit(message);
                return;
            }

            try
            {
                var results = await Task.WhenAll(couple.HandlerTypes.Select(handlerType =>
                {
                    var scope = _serviceProvider.CreateScope();
                    var handler = (IMessageHandler)scope.ServiceProvider.GetService(handlerType);
                    return handler.HandleAsync(deserializedData, cancellationToken)
                        .ContinueWith(t =>
                        {
                            scope.Dispose();
                            return t.Result;
                        }, CancellationToken.None);
                }));
                if (results.All(r => AllowedToCommitStatuses.Contains(r)))
                {
                    consumer.Commit(message);
                }
                else
                {
                    consumer.Seek(message.TopicPartitionOffset);
                    await Task.Delay(TimeSpan.FromMilliseconds(_coolDownInterval), cancellationToken);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error handling. Seeking back offset {offset} partition {partition}", message.Offset.Value, message.Partition.Value);
                consumer.Seek(message.TopicPartitionOffset);
                await Task.Delay(TimeSpan.FromMilliseconds(_coolDownInterval), cancellationToken);
            }
        }
    }
}