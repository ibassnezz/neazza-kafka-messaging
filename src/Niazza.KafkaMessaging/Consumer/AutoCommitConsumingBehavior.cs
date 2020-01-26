using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Niazza.KafkaMessaging.Consumer
{
    internal class AutoCommitConsumingBehavior: AbstractCommitConsumingBehavior
    {
        private readonly HandlersAggregationService _handlersAggregationService;

        public AutoCommitConsumingBehavior( 
            ILogger<AutoCommitConsumingBehavior> logger, 
            ISubscriberService subscriberService,
            HandlersAggregationService handlersAggregationService
            ): base(logger, subscriberService)
        {
            _handlersAggregationService = handlersAggregationService;
        }

        /// <summary>
        /// The result of handlers is not 
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task ConsumeAsync(IConsumer<Ignore, string> consumer, CancellationToken cancellationToken)
        {
            var consumeResult = consumer.Consume(cancellationToken);
            await _handlersAggregationService.HandleAsync(consumeResult.Topic, consumeResult.Message.Value,
                cancellationToken);
        }
    }
}