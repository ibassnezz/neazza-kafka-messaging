using System;

using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Niazza.KafkaMessaging;
using Niazza.KafkaMessaging.Producer;

namespace TestKafkaClient
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Producer");

            var serviceCollection = new ServiceCollection();
            
            serviceCollection.AddKafkaProducers(new ProducerConfiguration {Servers = "localhost:9092",
                AckType = AckToBrokersType.ToAllBrokers,
                Idempotency = MessageIdempotency.UseIdempotent});
            var provider = serviceCollection.BuildServiceProvider();
            var producerAsync = provider.GetService<IAsyncProducer>();
            var notify = new UnderNotify {Message = "OLA"};
            producerAsync.ProduceAsync(notify).GetAwaiter().GetResult();

            var producer = provider.GetService<IProducer>();

            var key = "";

            while ((key = Console.ReadLine()) != "wq")
            {
                producer.BeginProduce(new UnderNotify {Message = "OLA"});    
            }
            
            Console.ReadLine();
            provider.Dispose();
        }
    }
}