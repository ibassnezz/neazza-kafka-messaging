namespace Niazza.KafkaMessaging
{
    public interface IStatisticsPolling
    {
        void Poll(string statisticString);
    }
}