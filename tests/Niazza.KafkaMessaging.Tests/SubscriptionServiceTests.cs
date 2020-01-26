using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Niazza.KafkaMessaging.Consumer;
using Niazza.KafkaMessaging.Exceptions;

namespace Niazza.KafkaMessaging.Tests
{
    [TestClass]
    public class SubscriptionServiceTests
    {
        [TestMethod]
        public void AddHandler()
        {
            var subscriberService = new SubscriberService();
            subscriberService.Subscribe<TestMessageHandler, TestMessage>();

            var couple = subscriberService.GetMessageHandlersCouple(typeof(TestMessage)
                .GetCustomAttributes(typeof(KafkaMessageAttribute), true).Cast<KafkaMessageAttribute>().First().Topic);
            
            Assert.IsNotNull(couple);
            Assert.AreEqual(couple.HandlerTypes.Single(), typeof(TestMessageHandler));
            Assert.AreEqual(couple.MessageType, typeof(TestMessage));

        }
        
        [TestMethod]
        public void AddTwoDifferentHandlers()
        {
            
            var subscriberService = new SubscriberService();
            
            subscriberService.Subscribe<TestMessageHandler, TestMessage>();
            subscriberService.Subscribe<TestMessageSecondHandler, TestMessage>();
           
            var couple = subscriberService.GetMessageHandlersCouple(typeof(TestMessage)
                .GetCustomAttributes(typeof(KafkaMessageAttribute), true).Cast<KafkaMessageAttribute>().First().Topic);
            
            Assert.IsNotNull(couple);
            Assert.IsTrue(couple.HandlerTypes.Contains(typeof(TestMessageHandler)));
            Assert.IsTrue(couple.HandlerTypes.Contains(typeof(TestMessageSecondHandler)));
            Assert.AreEqual(2, couple.HandlerTypes.Count);
            Assert.AreEqual(couple.MessageType, typeof(TestMessage));

        }
        
        [TestMethod]
        public void SubcribeWithCustomTopic()
        {
            var subscriberService = new SubscriberService();
            subscriberService.Subscribe<IncorrectHandler, NoAttributeTestMessage>(topic:"Custom");

            Assert.IsTrue(subscriberService.GetTopicList("errors-").Contains("Custom"));
        }
        
        
        [TestMethod]
        [ExpectedException(typeof(SubscribeException))]
        public void ExpectedErrorWhileNoAttribute()
        {
            var subscriberService = new SubscriberService();
            subscriberService.Subscribe<IncorrectHandler, NoAttributeTestMessage>();
        }

    }
}
