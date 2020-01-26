using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Niazza.KafkaMessaging.Consumer;

namespace Niazza.KafkaMessaging.ErrorHandling.Strategies
{
    public class RepeatTillSuccessStrategy: IErrorHandlingStrategy
    {
        private readonly ILogger<RepeatTillSuccessStrategy> _logger;

        private const int Limit = 1000;

        public RepeatTillSuccessStrategy(ILogger<RepeatTillSuccessStrategy> logger)
        {
            _logger = logger;
        }
        
        public async Task<ExecutionResult> ExecutePlan(Func<Task<ExecutionResult>> handlerAction, 
            ErrorHandlingConfiguration configuration, 
            IDictionary<string, object> state,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return ExecutionResult.FailFinalized;
            
            if (state.ContainsKey(ErrorHandlingUtils.ErrorHandlingConstants.Attempts))
            {
                var attempts = Convert.ToInt32(state[ErrorHandlingUtils.ErrorHandlingConstants.Attempts]) + 1;
                state[ErrorHandlingUtils.ErrorHandlingConstants.Attempts] = attempts;
                if (attempts > Limit) return ExecutionResult.FailFinalized;
            }
            else
            {
                state.Add(ErrorHandlingUtils.ErrorHandlingConstants.Attempts, 1);
            }
            
            var delay = Task.Delay(
                TimeSpan.FromMilliseconds(
                    (int) configuration.Parameters[ErrorHandlingUtils.ErrorHandlingConstants.TimeIntervalMs]),
                cancellationToken);

            try
            {
                var result = await delay
                    .ContinueWith(async t => t.IsCanceled ? ExecutionResult.Cancelled : await handlerAction(),
                        cancellationToken).Unwrap();
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError("Error executing handling after error", e);
                return ExecutionResult.Failed;
            }
            
        }
        
    }
}