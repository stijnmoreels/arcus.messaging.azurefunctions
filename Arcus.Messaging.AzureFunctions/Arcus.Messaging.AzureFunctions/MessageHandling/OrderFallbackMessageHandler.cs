using System;
using System.Threading;
using System.Threading.Tasks;
using Arcus.Messaging.Abstractions;
using Arcus.Messaging.Pumps.ServiceBus;
using Arcus.Messaging.Pumps.ServiceBus.MessageHandling;
using Microsoft.Azure.ServiceBus;

namespace Arcus.Messaging.AzureFunctions.MessageHandling
{
    public class OrderFallbackMessageHandler : IAzureServiceBusFallbackMessageHandler
    {
        /// <summary>Process a new message that was received</summary>
        /// <param name="message">The Azure Service Bus Message message that was received</param>
        /// <param name="messageContext">The context providing more information concerning the processing</param>
        /// <param name="correlationInfo">
        ///     The information concerning correlation of telemetry and processes by using a variety of unique
        ///     identifiers.
        /// </param>
        /// <param name="cancellationToken">The cancellation token to cancel the processing.</param>
        public async Task ProcessMessageAsync(
            Message message,
            AzureServiceBusMessageContext messageContext,
            MessageCorrelationInfo correlationInfo,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}