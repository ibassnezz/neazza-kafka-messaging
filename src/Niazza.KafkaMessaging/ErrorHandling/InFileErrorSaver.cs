using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Niazza.KafkaMessaging.ErrorHandling
{
    public class InFileErrorSaver: IErrorSaver
    {
        private readonly ILogger<InFileErrorSaver> _logger;

        private const string UnhandledMessagesPath = "unhandled";

        public InFileErrorSaver(ILogger<InFileErrorSaver> logger)
        {
            _logger = logger;
        }

        public async Task SaveMassageAsync<TMessage>(TMessage message)
        {
            _logger.LogError("Message cannot be handled {@message}", message);

            Directory.CreateDirectory(UnhandledMessagesPath);

            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            using (var fs = new FileStream(
                Path.Combine(UnhandledMessagesPath, $"{typeof(TMessage).FullName}-{Guid.NewGuid():N}-UTC{DateTime.UtcNow:yyyy-MM-ddTHH-mm-ss-fff}"),
                FileMode.CreateNew,
                FileAccess.Write, FileShare.None, buffer.Length, true))
            {
                await fs.WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }
}