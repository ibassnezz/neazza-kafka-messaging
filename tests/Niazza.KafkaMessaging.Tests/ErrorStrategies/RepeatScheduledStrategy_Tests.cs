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
    public class RepeatScheduledStrategy_Tests
    {
        [TestMethod]
        public async Task ExecutePlan_IterationsTest()
        {
            var logging = new Mock<ILogger<RepeatScheduledStrategy>>();
            var strategy = new RepeatScheduledStrategy(logging.Object);

            var timeRange = new[] {1, 1, TimeSpan.FromMilliseconds(1).TotalMilliseconds, 1}; 

            var config = new ErrorHandlingConfiguration
            {
                Parameters = new Dictionary<string, object>() 
                    {{ErrorHandlingUtils.ErrorHandlingConstants.TimeRangeMs, timeRange }}
            };
            
            var state = new Dictionary<string, object>();

            for (var attempt = 0; attempt < timeRange.Length; attempt++)
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
        public async Task ExecutePlan_Test()
        {
            var logging = new Mock<ILogger<RepeatScheduledStrategy>>();
            var strategy = new RepeatScheduledStrategy(logging.Object);

            var config = new ErrorHandlingConfiguration
            {
                Parameters = new Dictionary<string, object>() 
                    {{ErrorHandlingUtils.ErrorHandlingConstants.TimeRangeMs, new[] {1, 1 }}}
            };
            
            var state = new Func<Dictionary<string, object>>(() => new Dictionary<string, object>{{ErrorHandlingUtils.ErrorHandlingConstants.Attempts,1}});

            var midResult = await strategy.ExecutePlan(() => Task.FromResult(ExecutionResult.Acknowledged), config, state(), CancellationToken.None);
            Assert.AreEqual(ExecutionResult.Acknowledged, midResult);
            
            midResult = await strategy.ExecutePlan(() => Task.FromResult(ExecutionResult.Cancelled), config, state(), CancellationToken.None);
            Assert.AreEqual(ExecutionResult.Cancelled, midResult);
            
            midResult = await strategy.ExecutePlan(() => Task.FromResult(ExecutionResult.PassOut), config, state(), CancellationToken.None);
            Assert.AreEqual(ExecutionResult.PassOut, midResult);
            
            midResult = await strategy.ExecutePlan(() => Task.FromResult(ExecutionResult.Failed), config, state(), CancellationToken.None);
            Assert.AreEqual(ExecutionResult.Failed, midResult);
            
            midResult = await strategy.ExecutePlan(() => Task.FromResult(ExecutionResult.FailFinalized), config, state(), CancellationToken.None);
            Assert.AreEqual(ExecutionResult.FailFinalized, midResult);
            
            midResult = await strategy.ExecutePlan(() => throw new Exception(), config, state(), CancellationToken.None);
            Assert.AreEqual(ExecutionResult.Failed, midResult);
            
            
            
        }

        [TestMethod]
        public async Task ExecutePlan_CancelledTest()
        {
            
            var logging = new Mock<ILogger<RepeatScheduledStrategy>>();
            var strategy = new RepeatScheduledStrategy(logging.Object);
            
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var result = await strategy.ExecutePlan(() => throw new Exception(), null, null, cts.Token);
            Assert.AreEqual(ExecutionResult.FailFinalized, result);
            
        }
    }
}