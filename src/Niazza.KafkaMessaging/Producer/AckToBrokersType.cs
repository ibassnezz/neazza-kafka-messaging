namespace Niazza.KafkaMessaging.Producer
{
    public enum AckToBrokersType
    {
        ToAllBrokers = 0,
        ToLeader,
        UseDefaults
    }
}