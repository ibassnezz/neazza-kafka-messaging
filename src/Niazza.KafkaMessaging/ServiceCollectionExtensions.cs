using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Niazza.KafkaMessaging.Consumer;
using Niazza.KafkaMessaging.ErrorHandling;
using Niazza.KafkaMessaging.ErrorHandling.Strategies;
using Niazza.KafkaMessaging.Producer;

[assembly: InternalsVisibleTo("Niazza.KafkaMessaging.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Niazza.KafkaMessaging
{
    public static class WebHostCollectionExtensions
    {
        /// <summary>
        /// Registering a consumer.
        /// User IConsumerStarter.Run to begin consuming
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure">fill no default fields</param>
        /// <param name="containers">Here the handler container</param>
        /// <returns></returns>
        public static IServiceCollection AddKafkaConsumers(this IServiceCollection services,
            Action<ConsumerConfiguration> configure, IHandlerContainer[] containers)
        {

            var configuration = Configure(configure);

            var subscriberService = new SubscriberService();
            foreach (var container in containers)
            {
                subscriberService.Subscribe(
                    container.HandlerType,
                    container.MessageType,
                    container.Settings.ErrorHandlerConfiguration,
                    container.Settings.Topic,
                    container.Settings.Behavior,
                    container.Settings.AsyncHandlingIntervalMs,
                    container.Settings.Serialization);

                if (container.HandlerFunc != null)
                {
                    services.AddScoped(container.HandlerFunc);
                }
                else
                {
                    services.AddScoped(container.HandlerType);
                }
            }

            RegisterConsumers(services, Configure(configure), subscriberService);
            return services;
        }

        private static ConsumerConfiguration Configure(Action<ConsumerConfiguration> configure)
        {
            var configuration = new ConsumerConfiguration
            {
                ManualCommitIntervalMs = 1000,
                IsAutocommitErrorHandling = true,
                Behavior = MainConsumerBehavior.Hybrid,
                AsyncHandlingIntervalMs = 0,
                CancellationDelayMaxMs = 100,
                NoOffsetType = NoOffsetType.TakeLatest,
                ErrorTopicPrefix = "errors-"

            };
            configure(configuration);
            return configuration;
        }


        private static void RegisterConsumers(
            IServiceCollection services,
            ConsumerConfiguration configuration,
            SubscriberService subscriberService)
        {

            subscriberService.Subscribe<ErrorMessageHandler, FailedMessageWrapper>(
                topic: ErrorHandlingUtils.ToErrorTopic(configuration.GroupId, configuration.ErrorTopicPrefix));
            services.AddLogging();
            services.AddSingleton<ISubscriberService>(subscriberService);
            services.AddSingleton(configuration);
            services.AddSingleton<IConsumerStarter, MainConsumer>();

            services.AddSingleton<HandlersAggregationService>();
            services.AddSingleton<HandlerExecutor>();
            services.AddSingleton<IAsyncLoopbackProducer>(provider =>
                new MainProducer(new ProducerConfiguration { Servers = configuration.Servers},
                    provider.GetService<ILogger<MainProducer>>()));

            services.AddSingleton<ISafeProducer, SafeProducer>();
            services.AddScoped<ErrorMessageHandler>();

            // if is not overwritten 
            services.TryAddSingleton<IErrorSaver, InFileErrorSaver>();

            services.AddSingleton<RepeatOnceStrategy>();
            services.AddSingleton<RepeatScheduledStrategy>();
            services.AddSingleton<RepeatTillSuccessStrategy>();

            services.AddSingleton<IDictionary<RepeatHandlingType, IErrorHandlingStrategy>>(provider =>
                new Dictionary<RepeatHandlingType, IErrorHandlingStrategy>
                {
                    {RepeatHandlingType.RepeatOnce, provider.GetService<RepeatOnceStrategy>()},
                    {RepeatHandlingType.RepeatScheduled, provider.GetService<RepeatScheduledStrategy>()},
                    {RepeatHandlingType.RepeatTillSuccess, provider.GetService<RepeatTillSuccessStrategy>()},
                    {RepeatHandlingType.None, new NoneStrategy()}
                });

            services.AddSingleton<ErrorHandlingStrategyFactory>();
            services.AddSingleton<IStatisticsPolling, MockStatisticsPolling>();

            //Behaviors
            services.AddSingleton<ManualCommitConsumingBehavior>();
            services.AddSingleton<AutoCommitConsumingBehavior>();
            services.AddSingleton<HybridConsumingBehavior>();
            services.AddSingleton<IBehaviorsCollection, BehaviorsCollection>();

            services.AddSingleton<IErrorConsumer>(provider => new ErrorConsumer(
                provider.GetService<ConsumerConfiguration>(),
                provider.GetService<ILogger<ErrorConsumer>>(),
                provider.GetService<IStatisticsPolling>(),
                configuration.IsAutocommitErrorHandling ? 
                    (IConsumingBehavior)provider.GetService<AutoCommitConsumingBehavior>(): 
                    provider.GetService<ManualCommitConsumingBehavior>())
            );

            //Messaging bus subscription on fly
            services.AddSingleton<IMessagingBus, MessagingBus>();

        }

        /// <summary>
        /// Handlers should be instantly resolved as Scoped outsides
        /// They would NOT be added automatically
        /// User IConsumerStarter.Run to begin consuming
        /// Use this if you want to add consumers on fly
        /// Note! the consumer will be subscribed to the LAST partition message 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddKafkaConsumers(this IServiceCollection services,
            Action<ConsumerConfiguration> configure)
        {
            RegisterConsumers(services, Configure(configure), new SubscriberService());
            return services;
        }

        /// <summary>
        /// Gathering statistics
        /// </summary>
        /// <param name="services"></param>
        /// <param name="statisticsPolling"></param>
        /// <returns></returns>
        public static IServiceCollection AddKafkaStatisticPolling(this IServiceCollection services,
            IStatisticsPolling statisticsPolling)
        {
            services.AddSingleton(provider => statisticsPolling);
            return services;
        }

        /// <summary>
        /// Due to some error strategies errors can be lost anyway
        /// IErrorSaver can catch the message. Strongly recommended to implement
        /// Otherwise the failed message will be saved as file in the executing directory
        /// </summary>
        /// <param name="services"></param>
        /// <param name="errorSaver"></param>
        /// <returns></returns>
        public static IServiceCollection AddKafkaErrorRecovering(this IServiceCollection services,
            IErrorSaver errorSaver)
        {
            services.AddSingleton(provider => errorSaver);
            return services;
        }

        /// <summary>
        /// Single stable producer. Use it for client apps
        /// </summary>
        /// <param name="services"></param>
        /// <param name="producerConfiguration"></param>
        /// <returns></returns>
        public static IServiceCollection AddKafkaProducers(this IServiceCollection services,
            ProducerConfiguration producerConfiguration)
        {
            services.AddSingleton<MainProducer>();
            services.AddSingleton<IProducer>(provider => provider.GetService<MainProducer>());
            services.AddSingleton<IAsyncProducer>(provider => provider.GetService<MainProducer>());
            services.AddSingleton(producerConfiguration);
            services.AddLogging();

            return services;
        }
    }
}