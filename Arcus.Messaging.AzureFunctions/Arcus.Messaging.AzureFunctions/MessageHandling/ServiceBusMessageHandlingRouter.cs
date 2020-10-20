using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Arcus.Messaging.Abstractions;
using Arcus.Messaging.Pumps.Abstractions.MessageHandling;
using Arcus.Messaging.Pumps.ServiceBus;
using Arcus.Messaging.Pumps.ServiceBus.MessageHandling;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Arcus.Messaging.AzureFunctions.MessageHandling
{
    public class ServiceBusMessageHandlingRouter
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ServiceBusMessageHandlingRouter> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusMessageHandlingRouter"/> class.
        /// </summary>
        public ServiceBusMessageHandlingRouter(
            IServiceProvider serviceProvider,
            ILogger<ServiceBusMessageHandlingRouter> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task ProcessMessageAsync<TMessageContext>(
            MessageReceiver messageReceiver, Microsoft.Azure.ServiceBus.Message message, TMessageContext messageContext, MessageCorrelationInfo correlationInfo, CancellationToken cancellationToken)
            where TMessageContext : AzureServiceBusMessageContext
        {
            IEnumerable<MessageHandler> handlers = MessageHandler.SubtractFrom(_serviceProvider, _logger);

            if (!handlers.Any())
            {
                throw new InvalidOperationException(
                    $"Message pump cannot correctly process the message in the '{typeof(TMessageContext)}' "
                    + "because no 'IMessageHandler<,>' was registered in the dependency injection container. "
                    + $"Make sure you call the correct '.With...' extension on the {nameof(IServiceCollection)} during the registration of the message pump to register a message handler");
            }

            bool isProcessed = false;
            string messageBody = Encoding.UTF8.GetString(message.Body);

            foreach (MessageHandler handler in handlers)
            {
                isProcessed = await ProcessMessageAsync(handler, messageReceiver, messageBody, messageContext, correlationInfo, cancellationToken);
                if (isProcessed)
                {
                    return;
                }
            }

            var fallbackMessageHandler = _serviceProvider.GetService<IAzureServiceBusFallbackMessageHandler>();
            if (fallbackMessageHandler is null)
            {
                throw new InvalidOperationException(
                    "Message pump was not able to process the message");
            }

            await fallbackMessageHandler.ProcessMessageAsync(message, messageContext, correlationInfo, cancellationToken);
        }

        private async Task<bool> ProcessMessageAsync<TMessageContext>(
            MessageHandler handler, 
            MessageReceiver messageReceiver,
            string message, 
            TMessageContext messageContext, 
            MessageCorrelationInfo correlationInfo, 
            CancellationToken cancellationToken)
            where TMessageContext : AzureServiceBusMessageContext
        {
            bool canProcessMessage = handler.CanProcessMessage(messageContext);
            var messageType = (Type) handler.GetType().GetProperty("MessageType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(handler);
            bool tryDeserializeToMessageFormat = TryDeserializeToMessageFormat(message, messageType, out var result);

            if (canProcessMessage && tryDeserializeToMessageFormat)
            {
                if (result is null)
                {
                    return false;
                    
                }

                await PreProcessingAsync(handler, messageContext, messageReceiver);
                await handler.ProcessMessageAsync(result, messageContext, correlationInfo, cancellationToken);
                return true;
            }

            return false;
        }

        protected virtual bool TryDeserializeToMessageFormat(string message, Type messageType, out object? result)
        {
            var success = true;
            var jsonSerializer = new JsonSerializer
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };
            jsonSerializer.Error += (sender, args) =>
            {
                success = false;
                args.ErrorContext.Handled = true;
            };

            var value = JToken.Parse(message).ToObject(messageType, jsonSerializer);
            if (success)
            {
                result = value;
                return true;
            }

            result = null;
            return false;
        }

        protected virtual async Task PreProcessingAsync(
            MessageHandler handler,
            AzureServiceBusMessageContext messageContext,
            MessageReceiver messageReceiver)
        {
            if (handler.Service is AzureServiceBusMessageHandlerTemplate template)
            {
                template.GetType().GetMethod("SetMessageReceiver", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(template, new [] { messageReceiver });
                template.GetType().GetMethod("SetLockToken", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(template, new [] { messageContext.SystemProperties.LockToken });
            }
        }
    }
}