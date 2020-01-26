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
    public class RepeatOnceStrategy_Tests
    {
        [TestMethod]
        public async Task ExecutePlan_Test()
        {
            var config = new ErrorHandlingConfiguration
            {
                RepeatHandlingType = RepeatHandlingType.RepeatOnce,
                Parameters = new Dictionary<string, object>()
                {
                    {ErrorHandlingUtils.ErrorHandlingConstants.TimeIntervalMs, 1}
                }
            };
            
            var logger = new Mock<ILogger<RepeatOnceStrategy>>();
            var strategy = new RepeatOnceStrategy(logger.Object);

            var result = await strategy.ExecutePlan(() => Task.FromResult(ExecutionResult.FailFinalized), config, null, CancellationToken.None);
            Assert.AreEqual(ExecutionResult.FailFinalized, result);
            
            result = await strategy.ExecutePlan(() => Task.FromResult(ExecutionResult.Acknowledged), config, null, CancellationToken.None);
            Assert.AreEqual(ExecutionResult.Acknowledged, result);
            
            result = await strategy.ExecutePlan(() => Task.FromResult(ExecutionResult.Cancelled), config, null, CancellationToken.None);
            Assert.AreEqual(ExecutionResult.FailFinalized, result);
            
            result = await strategy.ExecutePlan(() => Task.FromResult(ExecutionResult.PassOut), config, null, CancellationToken.None);
            Assert.AreEqual(ExecutionResult.FailFinalized, result);
            
            result = await strategy.ExecutePlan(() => Task.FromResult(ExecutionResult.Failed), config, null, CancellationToken.None);
            Assert.AreEqual(ExecutionResult.FailFinalized, result);
            
            result = await strategy.ExecutePlan(() => throw new Exception(), config, null, CancellationToken.None);
            Assert.AreEqual(ExecutionResult.FailFinalized, result);
            
        }


        [TestMethod]
        public async Task ExecutePlan_CancelledTest()
        {
            var logger = new Mock<ILogger<RepeatOnceStrategy>>();
            var strategy = new RepeatOnceStrategy(logger.Object);
            
            var cts = new CancellationTokenSource();
            cts.Cancel();
            
            var result = await strategy.ExecutePlan(() => Task.FromResult(ExecutionResult.Acknowledged), null, null, cts.Token);
            Assert.AreEqual(ExecutionResult.FailFinalized, result);
            
        }



    }
}