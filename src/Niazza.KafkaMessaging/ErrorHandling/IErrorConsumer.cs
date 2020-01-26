using System;
using System.Threading;
using System.Threading.Tasks;

namespace Niazza.KafkaMessaging.ErrorHandling
{
    public interface IErrorConsumer: IDisposable
    {
        Task Run(CancellationToken cancellationToken);
    }
}