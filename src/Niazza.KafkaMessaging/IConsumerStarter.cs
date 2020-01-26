using System.Threading;
using System.Threading.Tasks;

namespace Niazza.KafkaMessaging
{
    public interface IConsumerStarter
    {
        /// <summary>
        /// Sync run of the method
        /// </summary>
        /// <returns></returns>
        void Run();
        
        
        /// <summary>
        /// If you assign cancellationToken, be aware to cancel it from source while close the app
        /// Because it can cause data loss!!!
        /// </summary>
        /// <param name="cancellationToken">Do not set cancellationToken without cancelling logic!</param>
        /// <returns></returns>
        Task RunAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}