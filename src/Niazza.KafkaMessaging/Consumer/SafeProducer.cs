using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Niazza.KafkaMessaging.ErrorHandling;
using Niazza.KafkaMessaging.Producer;

namespace Niazza.KafkaMessaging.Consumer
{
    internal class SafeProducer : ISafeProducer
    {
        private readonly ILogger<SafeProducer> _logger;
        private readonly IAsyncLoopbackProducer _producer;
        private readonly IErrorSaver _saver;

        public SafeProducer(ILogger<SafeProducer> logger, IAsyncLoopbackProducer producer, IErrorSaver saver)
        {
            _logger = logger;
            _producer = producer;
            _saver = saver;
        }
        
        public async Task ProduceSafeAsync<TMessage>(TMessage message, string topic) where TMessage: class
        {
            try
            {
                await _producer.ProduceAsync(message, CancellationToken.None, topic);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Cannot produce message");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _saver.SaveMassageAsync(message).ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            
        }
    }
}