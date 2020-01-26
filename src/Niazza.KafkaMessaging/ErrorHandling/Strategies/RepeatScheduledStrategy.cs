using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Niazza.KafkaMessaging.Consumer;

namespace Niazza.KafkaMessaging.ErrorHandling.Strategies
{
    public class RepeatScheduledStrategy: IErrorHandlingStrategy
    {
        private readonly ILogger<RepeatScheduledStrategy> _logger;

        public RepeatScheduledStrategy(ILogger<RepeatScheduledStrategy> logger)
        {
            _logger = logger;
        }
        
        public async Task<ExecutionResult> ExecutePlan(Func<Task<ExecutionResult>> handlerAction, ErrorHandlingConfiguration configuration, IDictionary<string, object> state,
            CancellationToken cancellationToken)
        {

            if (cancellationToken.IsCancellationRequested) return ExecutionResult.FailFinalized;

            var timeRange = configuration.Parameters[ErrorHandlingUtils.ErrorHandlingConstants.TimeRangeMs];
            var scheduledRange = (timeRange as double[]) ?? (timeRange as int[])?.Select(Convert.ToDouble)?.ToArray() ??
                                 throw new InvalidCastException($"Cannot convert TimeRangeMs {timeRange.GetType()}");

            var attempts = state.ContainsKey(ErrorHandlingUtils.ErrorHandlingConstants.Attempts)
                ? Convert.ToInt32(state[ErrorHandlingUtils.ErrorHandlingConstants.Attempts]) + 1
                : 1;

            if (attempts > scheduledRange.Length) return ExecutionResult.FailFinalized;

            state[ErrorHandlingUtils.ErrorHandlingConstants.Attempts] = attempts;

            var delay = Task.Delay(TimeSpan.FromMilliseconds(scheduledRange[attempts - 1]), cancellationToken); 
            
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