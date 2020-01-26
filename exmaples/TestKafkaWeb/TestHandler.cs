using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Niazza.KafkaMessaging.Consumer;

namespace TestKafkaWeb
{
    public class TestHandler: AbstractMessageHandler<Notify>
    {
        private readonly ILogger<TestHandler> _logger;

        public TestHandler(ILogger<TestHandler> logger)
        {
            _logger = logger;
        }
        
        protected override Task<ExecutionResult> HandleAsync(Notify message, CancellationToken cancellationToken)
        {
            Console.WriteLine(message.Message);
            _logger.LogInformation(message.Message);
            return Task.FromResult(ExecutionResult.Acknowledged);
        }
    }
}