using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Niazza.KafkaMessaging.Serializers;

namespace Niazza.KafkaMessaging.Producer
{
    internal class MainProducer : IAsyncProducer, IAsyncLoopbackProducer, IProducer
    {
        private volatile bool _disposed;

        private readonly ProducerConfiguration _configuration;
        private readonly ILogger<MainProducer> _logger;
        private readonly Lazy<IProducer<Null, string>> _producer;

        public MainProducer(ProducerConfiguration configuration, ILogger<MainProducer> logger)
        {
            _configuration = configuration;
            _logger = logger;
            var config = new ProducerConfig
            {
                BootstrapServers = configuration.Servers,
                Partitioner = Partitioner.Random,
                MessageTimeoutMs = 60000,
                MessageSendMaxRetries = 10
            };

            if (configuration.AckType != AckToBrokersType.UseDefaults && 
                configuration.Idempotency != MessageIdempotency.UseIdempotent)
            {
                switch (configuration.AckType)
                {
                    case AckToBrokersType.ToAllBrokers:
                        config.Acks = Acks.All;
                        break;
                    case AckToBrokersType.ToLeader:
                        config.Acks = Acks.Leader;
                        break;
                }
            }

            if (configuration.Idempotency == MessageIdempotency.UseIdempotent)
            {
                config.EnableIdempotence = true;
            }


            _producer = new Lazy<IProducer<Null, string>>(CreateProducer(config));

        }

        private IProducer<Null, string> CreateProducer(ProducerConfig config)
        {
            var builder = new ProducerBuilder<Null, string>(config);
            return builder.Build();
        }

        public async Task ProduceAsync<TMessage>(
            TMessage message, CancellationToken token = default(CancellationToken), string topic = null, IMessageSerialization serialization = null)
            where TMessage : class
        {
            var topicKey = GetTopicKey(message, topic);
            var currentSerialization = serialization ?? new JsonMessageSerialization();

            var result = await _producer.Value.ProduceAsync(topicKey, new Message<Null, string>()
                { Value = currentSerialization.Serialize(message) });
            _logger.LogInformation("{message} was sent on {partition} partition, {topic} topic, {offset} offset",
                result.Value, result.Partition.Value, result.Topic, result.Offset);
            
        }

        public void BeginProduce<TMessage>(TMessage message, string topic = null, bool ignorePostfix = false, IMessageSerialization serialization = null) where TMessage : class
        {
            var topicKey = GetTopicKey(message, topic);
            var currentSerialization = serialization ?? new JsonMessageSerialization();
            _producer.Value.Produce(topicKey, new Message<Null, string>
            { Value = currentSerialization.Serialize(message) },
                result => _logger.LogInformation(
                    "{message} was sent on {partition} partition, {topic} topic, {offset} offset", result.Value,
                    result.Partition.Value, result.Topic, result.Offset)
            );
        }

        private string GetTopicKey<TMessage>(TMessage message, string topic) where TMessage : class
        {
            if (string.IsNullOrEmpty(topic))
            {
                var attribute = message.GetType().GetCustomAttribute<KafkaMessageAttribute>();
                if (attribute is null)
                {
                    throw new ArgumentException($"The message has no {nameof(KafkaMessageAttribute)}");
                }

                topic = attribute.Topic;
            }

            return topic;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            lock (_producer)
            {
                if (_disposed)
                {
                    return;
                }

                if (!_producer.IsValueCreated) return;
                // wait till producer sends all awaiting messages to bus
                Task.Delay(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
                _producer.Value.Flush(TimeSpan.FromMilliseconds(1000));
                _producer.Value.Dispose();
                _disposed = true;
            }
        }

        ~MainProducer()
        {
            Dispose(false);
        }
    }
}