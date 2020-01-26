using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Niazza.KafkaMessaging.Consumer;

namespace TestKafkaConsumer
{

    public class AutoCommitTestHandler : AbstractMessageHandler<NotifyAutoCommit>
    {
        private readonly ILogger<AutoCommitTestHandler> _logger;

        public AutoCommitTestHandler(ILogger<AutoCommitTestHandler> logger)
        {
            _logger = logger;
        }
        
        protected override async Task<ExecutionResult> HandleAsync(NotifyAutoCommit message, CancellationToken cancellationToken)
        {
            Console.WriteLine("AUTO:" + message.Message);
            _logger.LogInformation(message.Message);
            if (message.Message == "bang")
                throw new Exception("BANG!");
            if (message.Message == "time")
                await Task.Delay(100000, cancellationToken);
            return ExecutionResult.Acknowledged;
        }
    }

    public class ManualCommitTestHandler : AbstractMessageHandler<NotifyManualCommit>
    {
        private readonly ILogger<ManualCommitTestHandler> _logger;

        public ManualCommitTestHandler(ILogger<ManualCommitTestHandler> logger)
        {
            _logger = logger;
        }

        protected override Task<ExecutionResult> HandleAsync(NotifyManualCommit message, CancellationToken cancellationToken)
        {
            Console.WriteLine("MANUAL:" + message.Message);
            _logger.LogInformation(message.Message);
            if (message.Message == "bang")
                Task.FromException(new Exception("BANG!"));

            return Task.FromResult(ExecutionResult.Acknowledged);
        }
    }

    public class HybridTestHandler : AbstractMessageHandler<NotifyHybrid>
    {
        private readonly ILogger<HybridTestHandler> _logger;

        public HybridTestHandler(ILogger<HybridTestHandler> logger)
        {
            _logger = logger;
        }

        protected override Task<ExecutionResult> HandleAsync(NotifyHybrid message, CancellationToken cancellationToken)
        {
            Console.WriteLine("HYBRID:" + message.Message);
            _logger.LogInformation(message.Message);
            if (message.Message == "bang")
                Task.FromException(new Exception("BANG!"));

            return Task.FromResult(ExecutionResult.Acknowledged);
        }
    }

    public class RawTestHandler : AbstractMessageHandler<string>
    {
        
        protected override Task<ExecutionResult> HandleAsync(string message, CancellationToken cancellationToken)
        {
            Console.WriteLine("raw:" + message);
            return Task.FromResult(ExecutionResult.Acknowledged);
        }
    }


    public class XmlTestHandler : AbstractMessageHandler<NotifyXmlMessage>
    {

        protected override Task<ExecutionResult> HandleAsync(NotifyXmlMessage message, CancellationToken cancellationToken)
        {
            Console.WriteLine("xml:" + message.Message);
            return Task.FromResult(ExecutionResult.Acknowledged);
        }
    }



}