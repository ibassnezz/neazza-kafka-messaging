using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Niazza.KafkaMessaging.Consumer;

namespace Niazza.KafkaMessaging.ErrorHandling.Strategies
{
    public class RepeatOnceStrategy : IErrorHandlingStrategy
    {
        private readonly ILogger<RepeatOnceStrategy> _logger;

        public RepeatOnceStrategy(ILogger<RepeatOnceStrategy> logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Ack even if error 
        /// </summary>
        /// <param name="handlerAction"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ExecutionResult>  ExecutePlan(Func<Task<ExecutionResult>> handlerAction,
            ErrorHandlingConfiguration configuration, IDictionary<string, object> state,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return ExecutionResult.FailFinalized;
            var delay = Task.Delay(TimeSpan.FromMilliseconds((int)configuration.Parameters[ErrorHandlingUtils.ErrorHandlingConstants.TimeIntervalMs]), cancellationToken);

            try
            {
                var result = await delay
                    .ContinueWith(async t => t.IsCanceled ? ExecutionResult.Cancelled : await handlerAction(),
                        cancellationToken).Unwrap();
                
                return result == ExecutionResult.Acknowledged ? result : ExecutionResult.FailFinalized;
            }
            catch (Exception e)
            {
                _logger.LogError("Error executing handling after error", e);
            }

            return ExecutionResult.FailFinalized;
        }
    }
}