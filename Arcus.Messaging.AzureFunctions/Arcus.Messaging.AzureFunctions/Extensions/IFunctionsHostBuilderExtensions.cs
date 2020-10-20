// ReSharper disable once CheckNamespace

using System;
using Arcus.Messaging.AzureFunctions.MessageHandling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Azure.Functions.Extensions.DependencyInjection
{
    // ReSharper disable once InconsistentNaming
    public static class IFunctionsHostBuilderExtensions
    {
        public static IFunctionsHostBuilder WithMessageRouter(this IFunctionsHostBuilder builder, Func<IServiceProvider, ServiceBusMessageHandlingRouter> createImplementation)
        {
            builder.Services.AddSingleton(createImplementation);
            return builder;
        }

        public static IFunctionsHostBuilder WithMessageRouter(this IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton(serviceProvider =>
            {
                ILogger<ServiceBusMessageHandlingRouter> logger =
                    serviceProvider.GetService<ILogger<ServiceBusMessageHandlingRouter>>()
                    ?? NullLogger<ServiceBusMessageHandlingRouter>.Instance;

                return new ServiceBusMessageHandlingRouter(serviceProvider, logger);
            });

            return builder;
        }

        public static IFunctionsHostBuilder WithServiceBusMessageHandler<TMessageHandler, TMessage>(
            this IFunctionsHostBuilder builder)
        {
            builder.WithServiceBusMessageHandler<TMessageHandler, TMessage>();
            return builder;
        }

        public static IFunctionsHostBuilder WithServiceBusFallbackMessageHandler<TMessageHandler>(
            this IFunctionsHostBuilder builder)
        {
            builder.WithServiceBusFallbackMessageHandler<TMessageHandler>();
            return builder;
        }
    }
}