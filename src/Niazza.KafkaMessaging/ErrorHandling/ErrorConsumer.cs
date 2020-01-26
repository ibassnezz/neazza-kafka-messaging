using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Niazza.KafkaMessaging.Consumer;

namespace Niazza.KafkaMessaging.ErrorHandling
{
    public class ErrorConsumer: IErrorConsumer
    {
        private readonly ConsumerConfiguration _configuration;
        private readonly ILogger<ErrorConsumer> _logger;
        private readonly IStatisticsPolling _statisticsPolling;
        private readonly IConsumingBehavior _consumingBehavior;
        private readonly string _errorTopic;

        private IConsumer<Ignore, string> _consumer;

        internal ErrorConsumer(ConsumerConfiguration configuration, 
            ILogger<ErrorConsumer> logger, 
            IStatisticsPolling statisticsPolling, IConsumingBehavior consumingBehavior)
        {
            _configuration = configuration;
            _logger = logger;
            _statisticsPolling = statisticsPolling;
            _consumingBehavior = consumingBehavior;
            _errorTopic = ErrorHandlingUtils.ToErrorTopic(configuration.GroupId, configuration.ErrorTopicPrefix);
        }
        
        public void Dispose()
        {
            if (_consumer == null) return;
            //to prevent AccessViolationError
            Task.Delay(TimeSpan.FromMilliseconds(_configuration.CancellationDelayMaxMs)).GetAwaiter().GetResult();
            _consumer.Close();
            _consumer.Dispose();
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration.Servers,
                GroupId = _configuration.GroupId,
                EnableAutoCommit = _configuration.IsAutocommitErrorHandling,
                StatisticsIntervalMs = 20000,
                SessionTimeoutMs = 6000,
                HeartbeatIntervalMs = 2000,
                ClientId = GetType().FullName,
                CancellationDelayMaxMs = _configuration.CancellationDelayMaxMs,
                AutoOffsetReset = _configuration.NoOffsetType == NoOffsetType.TakeLatest ?
                    AutoOffsetReset.Latest : AutoOffsetReset.Earliest,
            };

            cancellationToken.Register(() =>
            {
                _consumer?.Unsubscribe();
            });
            
            var builder = new ConsumerBuilder<Ignore, string>(config);
            builder.SetErrorHandler((sender, s) => _logger.LogError("ErrorConsuming error raised: {@errorEvent}", s));
            builder.SetStatisticsHandler((sender, s) => _statisticsPolling.Poll(s));
            
            _consumer = builder.Build();

            _consumer.Subscribe(_errorTopic);

            await _consumingBehavior.RunConsumePollingAsync(_consumer, cancellationToken);

        }
    }
}