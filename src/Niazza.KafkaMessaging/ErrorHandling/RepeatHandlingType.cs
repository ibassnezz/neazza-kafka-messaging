namespace Niazza.KafkaMessaging.ErrorHandling
{
    public enum RepeatHandlingType
    {
        RepeatTillSuccess,
        RepeatOnce,
        RepeatScheduled,
        None
    }
}