using System;
using System.Threading;
using System.Threading.Tasks;
using Arcus.Messaging.Abstractions;
using Arcus.Messaging.AzureFunctions.Message;
using Arcus.Messaging.Pumps.ServiceBus;

namespace Arcus.Messaging.AzureFunctions.MessageHandling
{
    public class OrderMessageHandler : IAzureServiceBusMessageHandler<Order>
    {
        /// <summary>Process a new message that was received</summary>
        /// <param name="message">Message that was received</param>
        /// <param name="messageContext">Context providing more information concerning the processing</param>
        /// <param name="correlationInfo">
        ///     Information concerning correlation of telemetry and processes by using a variety of unique
        ///     identifiers
        /// </param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task ProcessMessageAsync(
            Order message,
            AzureServiceBusMessageContext messageContext,
            MessageCorrelationInfo correlationInfo,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}