using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Niazza.KafkaMessaging.ErrorHandling;

namespace TestKafkaWeb
{
    public class ErrorSaver: IErrorSaver
    {
        public async Task SaveMassageAsync<TMessage>(TMessage message)
        {
            Directory.CreateDirectory("unhandled");

            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            using (var fs = new FileStream(
                Path.Combine("unhandled", $"{Guid.NewGuid():N}-UTC{DateTime.UtcNow:yyyy-MM-ddTHH-mm-ss-fff}"),
                FileMode.CreateNew,
                FileAccess.Write, FileShare.None, buffer.Length, true))
            {
                await fs.WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }
}