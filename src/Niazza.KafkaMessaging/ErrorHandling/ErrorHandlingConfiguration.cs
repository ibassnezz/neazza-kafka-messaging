using System;
using System.Collections.Generic;

namespace Niazza.KafkaMessaging.ErrorHandling
{
    public class ErrorHandlingConfiguration
    {
        public RepeatHandlingType RepeatHandlingType { get; set; }
        
        public Dictionary<string, object> Parameters { get; set; }
        
        public static ErrorHandlingConfiguration GetRepeatOnce()
        {
            return new ErrorHandlingConfiguration
            {
                RepeatHandlingType = RepeatHandlingType.RepeatOnce,
                Parameters = new Dictionary<string, object>()
                {
                    {ErrorHandlingUtils.ErrorHandlingConstants.TimeIntervalMs, 5000}
                }
            };
        }

        public static ErrorHandlingConfiguration GetScheduled()
        {
            return new ErrorHandlingConfiguration
            {
                RepeatHandlingType = RepeatHandlingType.RepeatScheduled,
                Parameters = new Dictionary<string, object>{ { ErrorHandlingUtils.ErrorHandlingConstants.TimeRangeMs, new []
                {
                    100, 100, 200, 300, 500, 800, 
                    TimeSpan.FromSeconds(1).TotalMilliseconds, 
                    TimeSpan.FromSeconds(1).TotalMilliseconds, 
                    TimeSpan.FromSeconds(2).TotalMilliseconds, 
                    TimeSpan.FromSeconds(3).TotalMilliseconds, 
                    TimeSpan.FromSeconds(5).TotalMilliseconds,
                    TimeSpan.FromSeconds(8).TotalMilliseconds,
                    TimeSpan.FromSeconds(13).TotalMilliseconds,
                    TimeSpan.FromSeconds(21).TotalMilliseconds,
                    TimeSpan.FromSeconds(34).TotalMilliseconds,
                    TimeSpan.FromMinutes(1).TotalMilliseconds,
                    TimeSpan.FromMinutes(1).TotalMilliseconds,
                    TimeSpan.FromMinutes(2).TotalMilliseconds,
                    TimeSpan.FromMinutes(3).TotalMilliseconds,
                    TimeSpan.FromMinutes(5).TotalMilliseconds,
                    TimeSpan.FromMinutes(8).TotalMilliseconds,
                    TimeSpan.FromMinutes(13).TotalMilliseconds,
                    TimeSpan.FromMinutes(21).TotalMilliseconds,
                    TimeSpan.FromMinutes(34).TotalMilliseconds,
                    TimeSpan.FromHours(1).TotalMilliseconds,
                    TimeSpan.FromHours(1).TotalMilliseconds,
                    TimeSpan.FromHours(2).TotalMilliseconds,
                    TimeSpan.FromHours(3).TotalMilliseconds,
                    TimeSpan.FromHours(5).TotalMilliseconds,
                    TimeSpan.FromHours(8).TotalMilliseconds,
                    TimeSpan.FromHours(13).TotalMilliseconds,
                    TimeSpan.FromHours(21).TotalMilliseconds,
                    TimeSpan.FromHours(34).TotalMilliseconds,
                    
                    
                    
                }   } }
            };
        }

        public static ErrorHandlingConfiguration GetTillSuccess()
        {
            return new ErrorHandlingConfiguration
            {
                RepeatHandlingType = RepeatHandlingType.RepeatTillSuccess,
                Parameters = new Dictionary<string, object>{ { ErrorHandlingUtils.ErrorHandlingConstants.TimeIntervalMs, 5000} }
            };
        }

        public static ErrorHandlingConfiguration GetNone()
        {
            return new ErrorHandlingConfiguration
            {
                RepeatHandlingType = RepeatHandlingType.None
            };
        }
        
        
        
        
    }
}