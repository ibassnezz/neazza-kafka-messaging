using System;

namespace Niazza.KafkaMessaging.Exceptions
{
    public class SubscribeException: Exception
    {
        public SubscribeException(string message): base(message)
        {
            
        }
    }
}