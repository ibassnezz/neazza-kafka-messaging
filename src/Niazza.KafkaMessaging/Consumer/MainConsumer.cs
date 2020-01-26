using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Niazza.KafkaMessaging.ErrorHandling;

namespace Niazza.KafkaMessaging.Consumer
{
    internal class MainConsumer : IConsumerStarter, IDisposable
    {
        private readonly ISubscriberService _subscriberService;
        private readonly ILogger<MainConsumer> _logger;
        private readonly IStatisticsPolling _statisticsPolling;
        private readonly IBehaviorsCollection _behaviorsCollection;
        private readonly IErrorConsumer _errorConsumer;
        private readonly ConsumerConfiguration _configuration;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly Dictionary<MainConsumerBehavior, IConsumer<Ignore, string>> _consumers =
            new Dictionary<MainConsumerBehavior, IConsumer<Ignore, string>>();

        public MainConsumer(ConsumerConfiguration configuration,
            ISubscriberService subscriberService,
            ILogger<MainConsumer> logger,
            IStatisticsPolling statisticsPolling,
            IBehaviorsCollection behaviorsCollection,
            IErrorConsumer errorConsumer)
        {
            _configuration = configuration;
            _subscriberService = subscriberService;
            _logger = logger;
            _statisticsPolling = statisticsPolling;
            _behaviorsCollection = behaviorsCollection;
            _errorConsumer = errorConsumer;
        }
        
        public void Run()
        {
            RunAsync(_cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public Task RunAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);

            var currentCancellationToken =
                default(CancellationToken).Equals(cancellationToken)
                    ? _cancellationTokenSource.Token
                    : cancellationToken;
            currentCancellationToken.Register(Unsubscribe);

            SubscribeToTopics();

            foreach (var consumer in _consumers)
            {
                Task.Run(() => _behaviorsCollection.Get(consumer.Key).RunConsumePollingAsync(consumer.Value, currentCancellationToken),
                    currentCancellationToken).ConfigureAwait(false);
            }

            Task.Run(() => _errorConsumer.Run(currentCancellationToken)).ConfigureAwait(false);

            return Task.CompletedTask;
        }

        private void SubscribeToTopics()
        {
            var topics = _subscriberService.GetTopicList(_configuration.ErrorTopicPrefix).ToArray();

            if (!topics.Any())
            {
                _logger.LogWarning("No subscribes found at the beginning");
            }
            else
            {
                var groupedTopicsByBehavior = topics.Select(t =>
                        new
                        {
                            topic = t,
                            behavior = _subscriberService.GetMessageHandlersCouple(t)?.Behavior ??
                                       _configuration.Behavior
                        })
                    .GroupBy(tb => tb.behavior).Where(x => x.Any());


                foreach (var behaviorGroup in groupedTopicsByBehavior)
                {
                    var consumer = ProvideConsumer(behaviorGroup.Key);
                    consumer.Subscribe(behaviorGroup.Select(x => x.topic));
                }
            }
        }

        private IConsumer<Ignore, string> ProvideConsumer(MainConsumerBehavior behavior)
        {
            if (_consumers.TryGetValue(behavior, out var consumer)) return consumer;

            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration.Servers,
                GroupId = _configuration.GroupId,
                EnableAutoCommit = behavior == MainConsumerBehavior.AutoCommit,
                StatisticsIntervalMs = 20000,
                SessionTimeoutMs = 6000,
                AutoOffsetReset = _configuration.NoOffsetType == NoOffsetType.TakeLatest? 
                    AutoOffsetReset.Latest: AutoOffsetReset.Earliest,
                HeartbeatIntervalMs = 2000,
                ClientId = $"{GetType().FullName}::{behavior:G}",
                CancellationDelayMaxMs = _configuration.CancellationDelayMaxMs,
            };

            var builder = new ConsumerBuilder<Ignore, string>(config);
            builder.SetStatisticsHandler(OnOnStatistics);
            builder.SetErrorHandler(OnError);

            consumer = builder.Build();
            _consumers.Add(behavior, consumer);

            return consumer;
        }

        private void OnOnStatistics(object sender, string e)
        {
            _statisticsPolling.Poll(e);
        }

        private void OnError(IConsumer<Ignore, string> consumer, Error error)
        {
            _logger.LogError("Error consuming {consumer}: {@errorEvent}", consumer.Name, error);
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();

            foreach (var consumer in _consumers)
            {
                try
                {
                    ////to prevent AccessViolationError
                    Task.Delay(TimeSpan.FromMilliseconds(_configuration.CancellationDelayMaxMs)).GetAwaiter().GetResult();
                    consumer.Value.Close();
                    consumer.Value.Dispose();
                }
                catch (Exception exception)
                {
                    _logger.LogCritical(exception, "Incorrect consumer shutdown for {consumer}", consumer.Key.ToString("G"));
                }
            }
        }

        private void Unsubscribe()
        {
            try
            {
                Array.ForEach(_consumers.Values.ToArray(), c => c.Unsubscribe());
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unsubscribe exception");
            }
        }
    }
}