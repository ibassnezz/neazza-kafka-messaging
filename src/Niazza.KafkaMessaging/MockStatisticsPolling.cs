namespace Niazza.KafkaMessaging
{
    public class MockStatisticsPolling: IStatisticsPolling
    {
        public void Poll(string statisticString) { }
    }
}