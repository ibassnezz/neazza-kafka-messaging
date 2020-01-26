using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Niazza.KafkaMessaging;
using Niazza.KafkaMessaging.Consumer;
using Niazza.KafkaMessaging.ErrorHandling;

namespace TestKafkaWeb
{
    public class Startup
    {

        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddKafkaConsumers(configuration =>
            {
                configuration.Servers = "localhost:9092";
                configuration.GroupId = "test_group";
            }, new IHandlerContainer[]
            {
                new HandlerContainer<TestHandler, Notify>(
                    new CustomizedHandlerSettings(
                        new Dictionary<ExecutionResult, ErrorHandlingConfiguration> {{ExecutionResult.Failed, ErrorHandlingConfiguration.GetScheduled()}},
                        behavior:MainConsumerBehavior.Hybrid)),
            });
            services.AddKafkaStatisticPolling(new MockStatisticsPolling());
            services.AddKafkaErrorRecovering(new ErrorSaver());
        }


        public void Configure(IApplicationBuilder app, IServiceProvider provider, IApplicationLifetime lifetime)
        {
            var starter = provider.GetService<IConsumerStarter>();
            lifetime.ApplicationStarted.Register(() => starter.RunAsync(_tokenSource.Token));
            lifetime.ApplicationStopping.Register(() => _tokenSource.Cancel());
        }
   }
}