namespace Niazza.KafkaMessaging.ErrorHandling
{
    public static class ErrorHandlingUtils
    {
        public static class ErrorHandlingConstants
        {
            public const string TimeIntervalMs = "timeIntervalMs";
            public const string TimeRangeMs = "timeRangeMs";
            public const string Attempts = "attempts";
        }

        public static string ToErrorTopic(string topic, string errorPrefix) => $"{errorPrefix}{topic}";

    }
}