using System.Threading.Tasks;

namespace Niazza.KafkaMessaging.ErrorHandling
{
    public interface IErrorSaver
    {
        Task SaveMassageAsync<TMessage>(TMessage message);
    }
}