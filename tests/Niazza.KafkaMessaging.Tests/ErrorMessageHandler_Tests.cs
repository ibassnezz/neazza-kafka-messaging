using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Niazza.KafkaMessaging.Consumer;
using Niazza.KafkaMessaging.ErrorHandling;
using Niazza.KafkaMessaging.ErrorHandling.Strategies;

namespace Niazza.KafkaMessaging.Tests
{
    [TestClass]
    public class ErrorMessageHandler_Tests
    {
        [TestMethod]
        public async Task HandleAsync()
        {
            var dependencyResolver = new Mock<IServiceProvider>();
            var subscriberServer  = new Mock<ISubscriberService>();
            var logger = new Mock<ILogger<ErrorMessageHandler>>();
            var producer = new Mock<ISafeProducer>();
            var errorSaver = new Mock<IErrorSaver>();

            errorSaver.Setup(x => x.SaveMassageAsync(It.IsAny<FailedMessageWrapper>())).Returns(() => Task.CompletedTask);

            producer.Setup(p => p.ProduceSafeAsync(It.IsAny<FailedMessageWrapper>(),
                ErrorHandlingUtils.ToErrorTopic(typeof(TestMessage).FullName, "errors-"))).Returns(() => Task.CompletedTask);
            
            var message = new TestMessage
            {
                Number = "1234"
            };

            var errorMessageWrapper = new FailedMessageWrapper
            {
                ErrorMessage = "ERROR",
                HandlerName = typeof(TestMessageHandler).FullName,
                LastExecutionResult = ExecutionResult.Failed,
                Payload = JsonConvert.SerializeObject(message, new SerializingSettings()),
                State = new Dictionary<string, object>(),
                Topic = message.GetType().FullName,
                UtcFailedDate = DateTime.UtcNow

            };
            var strategy = new RepeatOnceStrategy(new Mock<ILogger<RepeatOnceStrategy>>().Object);
            var factory = new ErrorHandlingStrategyFactory(new Dictionary<RepeatHandlingType, IErrorHandlingStrategy>
            {
                {RepeatHandlingType.RepeatOnce, strategy},
                {RepeatHandlingType.RepeatScheduled, strategy},
                {RepeatHandlingType.RepeatTillSuccess, strategy},
                {RepeatHandlingType.None, new NoneStrategy()}
            });
            
            var defaultConfiguration = new Dictionary<ExecutionResult, ErrorHandlingConfiguration>
            {
                {ExecutionResult.Cancelled, ErrorHandlingConfiguration.GetRepeatOnce()},
                {ExecutionResult.Failed, ErrorHandlingConfiguration.GetRepeatOnce()},
                {ExecutionResult.PassOut, ErrorHandlingConfiguration.GetRepeatOnce()}
            };

            subscriberServer.Setup(s => s.GetMessageHandlersCouple(typeof(TestMessage).FullName))
                .Returns(() => new MessageHandlersCouple(typeof(TestMessage), new[] {typeof(TestMessageHandler)},
                    defaultConfiguration));
            
            var mock = new Mock<IMessageHandler>();
            var serviceScope = new Mock<IServiceScope>();
            var scFactory = new Mock<IServiceScopeFactory>();
            scFactory.Setup(sc => sc.CreateScope()).Returns(() => serviceScope.Object);
            var serviceProvider = new Mock<IServiceProvider>();
                
            serviceProvider.Setup(d => d.GetService(mock.Object.GetType())).Returns(() => mock.Object);
            serviceScope.SetupGet(s => s.ServiceProvider);

            var handler = new ErrorMessageHandler(subscriberServer.Object, logger.Object,
                factory, producer.Object, errorSaver.Object, new ConsumerConfiguration{IsAutocommitErrorHandling = true}, dependencyResolver.Object);

            var result = await handler.HandleAsync(errorMessageWrapper, CancellationToken.None);
            
            Assert.AreEqual(ExecutionResult.Acknowledged, result);
            
            subscriberServer.Verify(x => x.GetMessageHandlersCouple(typeof(TestMessage).FullName),Times.Once);
          
            
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var newResult = await handler.HandleAsync(errorMessageWrapper, cts.Token);
            Assert.AreEqual(ExecutionResult.Cancelled, newResult);
        }
    }
}