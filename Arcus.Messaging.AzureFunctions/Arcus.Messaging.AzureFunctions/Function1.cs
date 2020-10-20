using System.Threading;
using System.Threading.Tasks;
using Arcus.Messaging.Abstractions;
using Arcus.Messaging.AzureFunctions.MessageHandling;
using Arcus.Messaging.Pumps.ServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Arcus.Messaging.AzureFunctions
{
    public class Function1
    {
        private readonly ServiceBusMessageHandlingRouter _router;

        /// <summary>
        /// Initializes a new instance of the <see cref="Function1"/> class.
        /// </summary>
        public Function1(ServiceBusMessageHandlingRouter router)
        {
            _router = router;
        }

        [FunctionName("Function1")]
        public async Task Run(
            [ServiceBusTrigger("myqueue", Connection = "")]string myQueueItem, 
            Microsoft.Azure.ServiceBus.Message message, 
            MessageReceiver messageReceiver,
            ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            var context = new AzureServiceBusMessageContext(message.MessageId, message.SystemProperties, message.UserProperties);
            MessageCorrelationInfo correlation = message.GetCorrelationInfo();

            await _router.ProcessMessageAsync(messageReceiver, message, context, correlation, CancellationToken.None);
        }
    }
}
