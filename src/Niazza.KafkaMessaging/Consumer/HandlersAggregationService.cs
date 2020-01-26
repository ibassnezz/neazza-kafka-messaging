using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Niazza.KafkaMessaging.ErrorHandling;

namespace Niazza.KafkaMessaging.Consumer
{
    internal class HandlersAggregationService
    {
        private readonly ISubscriberService _subscriberService;
        private readonly HandlerExecutor _handlerExecutor;
        private readonly ILogger<HandlersAggregationService> _logger;
        private readonly IErrorSaver _errorSaver;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConsumerConfiguration _configuration;

        public HandlersAggregationService(
            ISubscriberService subscriberService,
            HandlerExecutor handlerExecutor,
            ILogger<HandlersAggregationService> logger,
            IErrorSaver errorSaver, 
            IServiceProvider serviceProvider,
            ConsumerConfiguration configuration
            )
        {
            _subscriberService = subscriberService;
            _handlerExecutor = handlerExecutor;
            _logger = logger;
            _errorSaver = errorSaver;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public async Task HandleAsync(string topicName, string message, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("New msg received {topic} {message}", topicName, message);
                var couple = _subscriberService.GetMessageHandlersCouple(topicName);
                if (couple == null)
                {
                    _logger.LogWarning("Cannot find subscribers for topic {topic}. It will be saved as error failed", topicName);

                    await _errorSaver.SaveMassageAsync(new FailedMessageWrapper()
                    {
                        ErrorMessage = "Not found",
                        Topic = topicName,
                        HandlerName = null,
                        LastExecutionResult = ExecutionResult.FailFinalized,
                        Payload = message,
                        State = new Dictionary<string, object>(),
                        UtcFailedDate = DateTime.UtcNow
                    });
                    return;
                }

                foreach (var handlerType in couple.HandlerTypes)
                {
                    var interval = _configuration.GetAsyncHandlingIntervalMs(couple.IntervalInMs);
                    var scope = _serviceProvider.CreateScope();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(() =>
                      {
                          var handler = (IMessageHandler)scope.ServiceProvider.GetService(handlerType);
                          return _handlerExecutor.ExecuteAsync(handler, message, couple,  interval, topicName, cancellationToken)
                              .ContinueWith(t => scope.Dispose(), CancellationToken.None);

                      }, CancellationToken.None).ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Cannot handle {topic} -> {@message}", topicName, message);
                throw;
            }
        }
    }
}