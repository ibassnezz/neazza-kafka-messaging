using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Niazza.KafkaMessaging.Consumer;

namespace Niazza.KafkaMessaging.ErrorHandling.Strategies
{
    public interface IErrorHandlingStrategy
    {
        Task<ExecutionResult> ExecutePlan(Func<Task<ExecutionResult>> handlerAction, ErrorHandlingConfiguration configuration,
            IDictionary<string, object> state, CancellationToken cancellationToken);
    }
}