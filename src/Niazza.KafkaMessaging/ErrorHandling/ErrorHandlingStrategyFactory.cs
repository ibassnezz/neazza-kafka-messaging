using System.Collections.Generic;
using Niazza.KafkaMessaging.ErrorHandling.Strategies;

namespace Niazza.KafkaMessaging.ErrorHandling
{
    internal class ErrorHandlingStrategyFactory
    {
        private readonly IDictionary<RepeatHandlingType, IErrorHandlingStrategy> _errorHandlingDictionary;

        public ErrorHandlingStrategyFactory(IDictionary<RepeatHandlingType, IErrorHandlingStrategy> errorHandlingDictionary)
        {
            _errorHandlingDictionary = errorHandlingDictionary;
        }
        
        
        public IErrorHandlingStrategy GetStrategy(ErrorHandlingConfiguration configuration)
        {
            return
                _errorHandlingDictionary.ContainsKey(configuration.RepeatHandlingType)
                    ? _errorHandlingDictionary[configuration.RepeatHandlingType]
                    : _errorHandlingDictionary[RepeatHandlingType.RepeatOnce];
        }
            
        
    }
}