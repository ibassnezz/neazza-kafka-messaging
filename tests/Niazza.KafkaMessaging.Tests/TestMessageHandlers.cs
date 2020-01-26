using System.Threading;
using System.Threading.Tasks;
using Niazza.KafkaMessaging.Consumer;

namespace Niazza.KafkaMessaging.Tests
{
    public class TestMessageHandler: AbstractMessageHandler<TestMessage>
    {
       protected override Task<ExecutionResult> HandleAsync(TestMessage message, CancellationToken cancellationToken)
       {
           return Task.FromResult(ExecutionResult.Acknowledged);
       }
    }
    
    public class TestMessageSecondHandler: AbstractMessageHandler<TestMessage>
    {
        protected override Task<ExecutionResult> HandleAsync(TestMessage message, CancellationToken cancellationToken)
        {
            return Task.FromResult(ExecutionResult.Acknowledged);
        }
    }

    public class IncorrectHandler : AbstractMessageHandler<NoAttributeTestMessage>
    {
        protected override Task<ExecutionResult> HandleAsync(NoAttributeTestMessage message, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}