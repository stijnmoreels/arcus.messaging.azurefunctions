using System.Collections.Generic;
using System.Text;
using Arcus.Messaging.AzureFunctions;
using Arcus.Messaging.AzureFunctions.Message;
using Arcus.Messaging.AzureFunctions.MessageHandling;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Arcus.Messaging.AzureFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.WithMessageRouter()
                   .WithServiceBusMessageHandler<OrderMessageHandler, Order>()
                   .WithServiceBusFallbackMessageHandler<OrderFallbackMessageHandler>();
        }
    }
}
