using System;
using System.Collections.Generic;
using Niazza.KafkaMessaging.Consumer;

namespace Niazza.KafkaMessaging.ErrorHandling
{
    public class FailedMessageWrapper
    {
        public string Topic { get; set; }
        
        public string HandlerName { get; set; }
        
        public string Payload { get; set; }
        
        public string ErrorMessage { get; set; }
        
        public DateTime UtcFailedDate { get; set; }
        
        public ExecutionResult LastExecutionResult { get; set; }
        
        public Dictionary<string, object> State { get; set; }
        
        
    }
}