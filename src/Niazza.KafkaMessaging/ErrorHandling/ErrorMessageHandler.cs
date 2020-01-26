using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Niazza.KafkaMessaging.Consumer;

namespace Niazza.KafkaMessaging.ErrorHandling
{
    internal class ErrorMessageHandler : AbstractMessageHandler<FailedMessageWrapper>
    {
        private readonly ISubscriberService _subscriberService;
        private readonly ILogger<ErrorMessageHandler> _logger;
        private readonly ErrorHandlingStrategyFactory _errorHandlingStrategyFactory;
        private readonly ISafeProducer _safeProducer;
        private readonly IErrorSaver _errorSaver;
        private readonly ConsumerConfiguration _consumerConfiguration;
        private readonly IServiceProvider _serviceProvider;

        public ErrorMessageHandler(ISubscriberService subscriberService,
            ILogger<ErrorMessageHandler> logger, 
            ErrorHandlingStrategyFactory errorHandlingStrategyFactory, 
            ISafeProducer safeProducer, 
            IErrorSaver errorSaver, 
            ConsumerConfiguration consumerConfiguration,
            IServiceProvider serviceProvider)
        {
            _subscriberService = subscriberService;
            _logger = logger;
            _errorHandlingStrategyFactory = errorHandlingStrategyFactory;
            _safeProducer = safeProducer;
            _errorSaver = errorSaver;
            _consumerConfiguration = consumerConfiguration;
            _serviceProvider = serviceProvider;
        }

        protected override async Task<ExecutionResult> HandleAsync(FailedMessageWrapper message,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Try handle failed message {@message}", message);

                if (cancellationToken.IsCancellationRequested) return ExecutionResult.Cancelled;

                var couple = _subscriberService.GetMessageHandlersCouple(message.Topic);
                if (couple == null)
                {
                    _logger.LogError("Cannot find couple(handler) for topic {topic}", message.Topic);
                    return ExecutionResult.Acknowledged;
                }

                var handlerType = couple.HandlerTypes.FirstOrDefault(c => c.FullName == message.HandlerName);
                if (handlerType == default(Type)) return ExecutionResult.Acknowledged;

                var typedMessage = couple.Deserialize(message.Payload);
                var strategy =
                    _errorHandlingStrategyFactory.GetStrategy(
                        couple.ErrorHandlingConfigurations[message.LastExecutionResult]);
                using (var scope = _serviceProvider.CreateScope())
                {
                    var handler = (IMessageHandler)scope.ServiceProvider.GetService(handlerType);
                    var result = await strategy.ExecutePlan(
                        () => handler.HandleAsync(typedMessage, cancellationToken),
                        couple.ErrorHandlingConfigurations[message.LastExecutionResult],
                        message.State, cancellationToken);

                    if (!_consumerConfiguration.IsAutocommitErrorHandling
                        || result == ExecutionResult.Acknowledged)
                        return result;

                    message.UtcFailedDate = DateTime.UtcNow;

                    if (result == ExecutionResult.FailFinalized)
                    {
                        await _errorSaver.SaveMassageAsync(message);
                        return ExecutionResult.Acknowledged;
                    }
                }
            }
            catch (JsonException jsonException)
            {
                _logger.LogError(jsonException, "Error message handling json Exception");
                message.ErrorMessage = JsonConvert.SerializeObject(jsonException);
                await _errorSaver.SaveMassageAsync(message);
                return ExecutionResult.Acknowledged;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error message handling exception");
                message.ErrorMessage = JsonConvert.SerializeObject(e);
            }
            
            await _safeProducer.ProduceSafeAsync(message, ErrorHandlingUtils.ToErrorTopic(_consumerConfiguration.GroupId, _consumerConfiguration.ErrorTopicPrefix));

            //Always send Ack because it has its own error processing
            return ExecutionResult.Acknowledged;
        }
    }
}