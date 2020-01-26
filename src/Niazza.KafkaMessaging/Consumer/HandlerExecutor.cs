using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Niazza.KafkaMessaging.ErrorHandling;

namespace Niazza.KafkaMessaging.Consumer
{
    internal class HandlerExecutor
    {

        private readonly ILogger<HandlerExecutor> _logger;
        private readonly ISafeProducer _safeProducer;
        private readonly ConsumerConfiguration _configuration;

        public HandlerExecutor(ILogger<HandlerExecutor> logger, ISafeProducer safeProducer, ConsumerConfiguration configuration)
        {
            _logger = logger;
            _safeProducer = safeProducer;
            _configuration = configuration;
        }
        
        public async Task ExecuteAsync(IMessageHandler handler, string serializedMessage, MessageHandlersCouple couple, int intervalMs, string topic, CancellationToken cancellationToken)
        {
            ExecutionResult result;
            Exception exception = null;
            try
            {
                if (intervalMs != 0)
                {
                    await Task.Delay(intervalMs, cancellationToken);
                }
                var payload = couple.Deserialize(serializedMessage);
                result = await handler.HandleAsync(payload, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning($"Task cancelled in handler {handler.GetType().FullName}");
                result = ExecutionResult.Cancelled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error in handler {handler.GetType().FullName}");
                result = ExecutionResult.Failed;
                exception = e;
            }
            
            if (result == ExecutionResult.Acknowledged || result == ExecutionResult.PassOut) return;

            var message = new FailedMessageWrapper
            {
                Topic = topic,
                HandlerName = handler.GetType().FullName,
                Payload = serializedMessage,
                UtcFailedDate = DateTime.UtcNow,
                State = new Dictionary<string, object>(),
                LastExecutionResult = result,
                ErrorMessage = exception != null? JsonConvert.SerializeObject(exception): null
            };

            await _safeProducer.ProduceSafeAsync(message, ErrorHandlingUtils.ToErrorTopic(_configuration.GroupId, _configuration.ErrorTopicPrefix));
            
        }
    }
}