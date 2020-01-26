using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Niazza.KafkaMessaging.ErrorHandling;

namespace Niazza.KafkaMessaging.Consumer
{
    internal class HybridConsumingBehavior : AbstractCommitConsumingBehavior
    {
        private readonly IErrorSaver _errorSaver;
        private readonly ISafeProducer _safeProducer;
        private readonly ConsumerConfiguration _consumerConfiguration;
        private readonly IServiceProvider _serviceProvider;

        public HybridConsumingBehavior(ILogger<HybridConsumingBehavior> logger, 
            ISubscriberService subscriberService, IErrorSaver errorSaver,
            ISafeProducer safeProducer, ConsumerConfiguration consumerConfiguration, 
            IServiceProvider serviceProvider) : base(logger, subscriberService)
        {
            _errorSaver = errorSaver;
            _safeProducer = safeProducer;
            _consumerConfiguration = consumerConfiguration;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ConsumeAsync(IConsumer<Ignore, string> consumer,
            CancellationToken cancellationToken)
        {
            var message = consumer.Consume(cancellationToken);
            try
            {
                Logger.LogInformation("New message received {topic} {message}", message.Topic, message.Value);
                var couple = SubscriberService.GetMessageHandlersCouple(message.Topic);
                if (couple == null)
                {
                    await _errorSaver.SaveMassageAsync(message);
                    //there is no chance to recover this message
                    return;
                }
                
                var deserializedData = couple.Deserialize(message.Value);

                foreach (var handlerType in couple.HandlerTypes)
                {
                    ExecutionResult result;
                    Exception exception = null;
                    try
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var handler = (IMessageHandler)scope.ServiceProvider.GetService(handlerType);
                            result = await handler.HandleAsync(deserializedData, cancellationToken);
                        }
                    }
                    catch (Exception e)
                    {
                        result = ExecutionResult.Failed;
                        Logger.LogError(e, "Error executing handler");
                        exception = e;
                    }

                    if (result == ExecutionResult.Acknowledged || result == ExecutionResult.PassOut) continue;

                    var errorMessage = new FailedMessageWrapper
                    {
                        Topic = message.Topic,
                        HandlerName = handlerType.FullName,
                        Payload = message.Value,
                        UtcFailedDate = DateTime.UtcNow,
                        State = new Dictionary<string, object>(),
                        LastExecutionResult = result,
                        ErrorMessage = exception != null ? JsonConvert.SerializeObject(exception) : null
                    };

                    await _safeProducer.ProduceSafeAsync(errorMessage,
                        ErrorHandlingUtils.ToErrorTopic(_consumerConfiguration.GroupId, _consumerConfiguration.ErrorTopicPrefix));
                }
            }
            finally
            {
                consumer.Commit(message);
            }
        }
    }
}