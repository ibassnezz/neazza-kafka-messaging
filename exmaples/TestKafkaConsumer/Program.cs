using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Niazza.KafkaMessaging;
using Niazza.KafkaMessaging.Consumer;
using Niazza.KafkaMessaging.ErrorHandling;
using Niazza.KafkaMessaging.Serializers;

namespace TestKafkaConsumer
{
    class Program
    {

        public static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder.AddConsole());

            serviceCollection.AddKafkaConsumers(configuration =>
            {
                configuration.Servers = "localhost:9092";
                configuration.GroupId = "test-group";
                configuration.IsAutocommitErrorHandling = true;
                configuration.ManualCommitIntervalMs = 1000;
                configuration.Behavior = MainConsumerBehavior.AutoCommit;
                configuration.AsyncHandlingIntervalMs = 100;
            }, new IHandlerContainer[]
            {
                new HandlerContainer<HybridTestHandler, NotifyHybrid>(
                    new CustomizedHandlerSettings(
                        new Dictionary<ExecutionResult, ErrorHandlingConfiguration> {{ExecutionResult.Failed, ErrorHandlingConfiguration.GetScheduled()}},
                        behavior:MainConsumerBehavior.Hybrid)),

                new HandlerContainer<AutoCommitTestHandler, NotifyAutoCommit>(
                    new CustomizedHandlerSettings(
                        new Dictionary<ExecutionResult, ErrorHandlingConfiguration> {{ExecutionResult.Failed, ErrorHandlingConfiguration.GetScheduled()}},
                        behavior:MainConsumerBehavior.AutoCommit,
                        asyncHandlingIntervalMs: 1000)),

                new HandlerContainer<ManualCommitTestHandler, NotifyManualCommit>(
                    new CustomizedHandlerSettings(
                        new Dictionary<ExecutionResult, ErrorHandlingConfiguration> {{ExecutionResult.Failed, ErrorHandlingConfiguration.GetScheduled()}},
                        behavior:MainConsumerBehavior.Manual)),

                new HandlerContainer<RawTestHandler, string>(
                    new CustomizedHandlerSettings(
                        new Dictionary<ExecutionResult, ErrorHandlingConfiguration> {{ExecutionResult.Failed, ErrorHandlingConfiguration.GetScheduled()}},
                        behavior:MainConsumerBehavior.AutoCommit, 
                        topic: "raw-topic",
                        serialization: new RawMessageSerialization())),

                new HandlerContainer<XmlTestHandler, NotifyXmlMessage>(
                    new CustomizedHandlerSettings(
                        new Dictionary<ExecutionResult, ErrorHandlingConfiguration> {{ExecutionResult.Failed, ErrorHandlingConfiguration.GetScheduled()}},
                        behavior:MainConsumerBehavior.AutoCommit,
                        serialization: new XmlMessageSerialization())),

            });
             
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var tokeSource = new CancellationTokenSource();
            var consumer = serviceProvider.GetService<IConsumerStarter>();
            consumer.RunAsync(tokeSource.Token);
            Console.WriteLine("Consumer");
            Console.ReadLine();

            tokeSource.Cancel();
            
            serviceProvider.Dispose();
        }
    }

}