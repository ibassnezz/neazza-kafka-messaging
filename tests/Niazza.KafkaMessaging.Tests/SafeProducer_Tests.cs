using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Niazza.KafkaMessaging.Consumer;
using Niazza.KafkaMessaging.ErrorHandling;
using Niazza.KafkaMessaging.Producer;

namespace Niazza.KafkaMessaging.Tests
{
    [TestClass]
    public class SafeProducer_Tests
    {
        
        [TestMethod]
        public async Task ProduceSafeAsync_CheckSavingMessageTest()
        {
            var logger = new Mock<ILogger<SafeProducer>>();
            var producer = new Mock<IAsyncLoopbackProducer>();
            producer.Setup(x =>
                x.ProduceAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>(), It.IsAny<string>(), null))
                .Returns(() => throw new Exception());
            var saver = new Mock<IErrorSaver>();
            saver.Setup(x => x.SaveMassageAsync(It.IsAny<TestMessage>())).Returns(() => Task.CompletedTask);
            
            var safeProducer = new SafeProducer(logger.Object, producer.Object, saver.Object);

            await safeProducer.ProduceSafeAsync(new TestMessage(), string.Empty);
            saver.Verify(mock => mock.SaveMassageAsync(It.IsAny<TestMessage>()), Times.Once);
            producer.Verify(pr => pr.ProduceAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>(), It.IsAny<string>(), null), Times.Once);
        }
        
        
        [TestMethod]
        public async Task ProduceSafeAsync_DefaultCaseTest()
        {
            var logger = new Mock<ILogger<SafeProducer>>();
            var producer = new Mock<IAsyncLoopbackProducer>();
            producer.Setup(x =>
                    x.ProduceAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>(), It.IsAny<string>(), null)).Returns(() => Task.CompletedTask);
            
            var saver = new Mock<IErrorSaver>();
            saver.Setup(x => x.SaveMassageAsync(It.IsAny<TestMessage>()));
            
            var safeProducer = new SafeProducer(logger.Object, producer.Object, saver.Object);
            await safeProducer.ProduceSafeAsync(new TestMessage(), string.Empty);
            
            producer.Verify(pr => pr.ProduceAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>(), It.IsAny<string>(), null), Times.Once);
            saver.Verify(mock => mock.SaveMassageAsync(It.IsAny<TestMessage>()), Times.Never);
        }
        
        
        
    }
}