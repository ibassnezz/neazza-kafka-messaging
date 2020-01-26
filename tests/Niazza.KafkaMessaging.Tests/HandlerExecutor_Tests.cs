using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Niazza.KafkaMessaging.Consumer;
using Niazza.KafkaMessaging.ErrorHandling;

namespace Niazza.KafkaMessaging.Tests
{
    [TestClass]
    public class HandlerExecutor_Tests
    {
        [TestMethod]
        public async Task Execute_DefaultTest()
        {

            var logger = new Mock<ILogger<HandlerExecutor>>();
            var safeProducer = new Mock<ISafeProducer>();
            safeProducer.Setup(t => t.ProduceSafeAsync(It.IsAny<FailedMessageWrapper>(), It.IsAny<string>())).Returns(() => Task.CompletedTask);
            
            var handler = new Mock<IMessageHandler>();
            
            var message = new TestMessage { Number = "1234"};
            
            handler.Setup(m => m.HandleAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(ExecutionResult.Acknowledged));
            
            var handlerExecutor = new HandlerExecutor(logger.Object, safeProducer.Object, new ConsumerConfiguration() { GroupId = "test" });

            await handlerExecutor.ExecuteAsync(handler.Object, JsonConvert.SerializeObject(message), new MessageHandlersCouple(message.GetType(), new List<Type>(), null), 0,
                message.GetType().FullName, CancellationToken.None);
                
            handler.Verify(x => x.HandleAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
                
            safeProducer.Verify(x => x.ProduceSafeAsync(It.IsAny<FailedMessageWrapper>(), It.IsAny<string>()), Times.Never);

        }

        [TestMethod]
        public async Task Execute_FailedFirstTest()
        {
            var message = new TestMessage { Number = "1234"};
            var errorPrefix = "errors-";
            var serializedMessage = JsonConvert.SerializeObject(message, new SerializingSettings());
            var logger = new Mock<ILogger<HandlerExecutor>>();
            var safeProducer = new Mock<ISafeProducer>();

            var executionResult = ExecutionResult.Failed;

            var handler = new Mock<IMessageHandler>();

            FailedMessageWrapper wrapper = null;
            var consumerConfiguration = new ConsumerConfiguration(){ GroupId = "Test", ErrorTopicPrefix = errorPrefix };
            safeProducer
                .Setup(t => t.ProduceSafeAsync(It.IsAny<FailedMessageWrapper>(),
                    ErrorHandlingUtils.ToErrorTopic(consumerConfiguration.GroupId, errorPrefix))).Returns(() => Task.CompletedTask)
                .Callback<FailedMessageWrapper, string>(((w, s) => { wrapper = w; }));
            
            
            handler.Setup(m => m.HandleAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(executionResult));

            
            var handlerExecutor = new HandlerExecutor(logger.Object, safeProducer.Object, consumerConfiguration);

            
            await handlerExecutor.ExecuteAsync(handler.Object, serializedMessage, new MessageHandlersCouple(message.GetType(), new List<Type>(), null), 0,
                message.GetType().FullName, CancellationToken.None);
                
            handler.Verify(x => x.HandleAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);

            safeProducer.Verify(
                x => x.ProduceSafeAsync(It.IsAny<FailedMessageWrapper>(), ErrorHandlingUtils.ToErrorTopic(consumerConfiguration.GroupId, errorPrefix)),
                Times.Once);
            
            Assert.IsNotNull(wrapper);
            
            Assert.AreEqual(serializedMessage, wrapper.Payload);
            Assert.AreNotEqual(default(DateTime), wrapper.UtcFailedDate);
            Assert.AreEqual(message.GetType().FullName, wrapper.Topic);
            Assert.AreEqual(executionResult, wrapper.LastExecutionResult);
            Assert.IsNotNull(wrapper.State);

        }
        
    }
}