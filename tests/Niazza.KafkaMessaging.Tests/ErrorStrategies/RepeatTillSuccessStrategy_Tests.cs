using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Niazza.KafkaMessaging.Consumer;
using Niazza.KafkaMessaging.ErrorHandling;
using Niazza.KafkaMessaging.ErrorHandling.Strategies;

namespace Niazza.KafkaMessaging.Tests.ErrorStrategies
{
    [TestClass]
    public class RepeatTillSuccessStrategy_Tests
    {
        [TestMethod]
        public async Task ExecutePlan_IterationsTest()
        {
            var logging = new Mock<ILogger<RepeatTillSuccessStrategy>>();
            var strategy = new RepeatTillSuccessStrategy(logging.Object);

            var config = new ErrorHandlingConfiguration
            {
                Parameters = new Dictionary<string, object>() 
                    {{ErrorHandlingUtils.ErrorHandlingConstants.TimeIntervalMs, 1 }}
            };
            
            var state = new Dictionary<string, object>();

            for (var attempt = 0; attempt < 1000; attempt++)
            {
                var midResult = await strategy.ExecutePlan(() => Task.FromResult(ExecutionResult.Failed), config, state, CancellationToken.None);
                Assert.AreEqual(attempt + 1, (int)state[ErrorHandlingUtils.ErrorHandlingConstants.Attempts]);
                Assert.AreEqual(ExecutionResult.Failed, midResult);
            }
            
            var result = await strategy.ExecutePlan(() => Task.FromResult(ExecutionResult.Failed), config, state, CancellationToken.None);
            Assert.AreEqual(ExecutionResult.FailFinalized, result);
            
            result = await strategy.ExecutePlan(() => Task.FromResult(ExecutionResult.Failed), config, state, CancellationToken.None);
            Assert.AreEqual(ExecutionResult.FailFinalized, result);
        }

        [TestMethod]
        public async Task ExecutePlan_ExceptionTest()
        {
            var logging = new Mock<ILogger<RepeatTillSuccessStrategy>>();
            var strategy = new RepeatTillSuccessStrategy(logging.Object);

            var config = new ErrorHandlingConfiguration
            {
                Parameters = new Dictionary<string, object>() 
                    {{ErrorHandlingUtils.ErrorHandlingConstants.TimeIntervalMs, 1 }}
            };
            
            var state = new Dictionary<string, object>();

            var result = await strategy.ExecutePlan(() => throw new Exception(), config, state, CancellationToken.None);
            Assert.AreEqual(1, (int)state[ErrorHandlingUtils.ErrorHandlingConstants.Attempts]);
            Assert.AreEqual(ExecutionResult.Failed, result);

        }
        
        [TestMethod]
        public async Task ExecutePlan_CancelTest()
        {
            var logging = new Mock<ILogger<RepeatTillSuccessStrategy>>();
            var strategy = new RepeatTillSuccessStrategy(logging.Object);

            var config = new ErrorHandlingConfiguration
            {
                Parameters = new Dictionary<string, object>() 
                    {{ErrorHandlingUtils.ErrorHandlingConstants.TimeIntervalMs, 1 }}
            };
            
            var state = new Dictionary<string, object>();
            
            var tcs = new CancellationTokenSource();
            tcs.Cancel();

            var result = await strategy.ExecutePlan(() => throw new Exception(), config, state, tcs.Token);
            
            Assert.AreEqual(ExecutionResult.FailFinalized, result);
            
        }
            
    }
}