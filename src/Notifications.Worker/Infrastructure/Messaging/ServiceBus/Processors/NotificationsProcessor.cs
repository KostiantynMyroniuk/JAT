using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Notifications.Worker.Infrastructure.Messaging.ServiceBus.Processors
{
    public class NotificationsProcessor(
        ServiceBusClient serviceBusClient,
        HandlerDispatcher handlerDispatcher,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<NotificationsProcessor> logger) : BackgroundService
    {
        private ServiceBusProcessor? _processor;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _processor = serviceBusClient.CreateProcessor("interviews", "notifications", new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 5,
                AutoCompleteMessages = true,
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
            });

            _processor.ProcessMessageAsync += OnMessageAsync;
            _processor.ProcessErrorAsync += OnErrorAsync;

            await _processor.StartProcessingAsync(stoppingToken);

            try { await Task.Delay(Timeout.Infinite, stoppingToken); }
            catch(OperationCanceledException) { }
        }

        private async Task OnMessageAsync(ProcessMessageEventArgs args)
        {
            var msg = args.Message;
            var scope = serviceScopeFactory.CreateAsyncScope();

            var handled = await handlerDispatcher.DispatchAsync(
                msg.Subject,
                msg.Body,
                scope.ServiceProvider,
                args.CancellationToken);

            if (!handled)
                await args.DeadLetterMessageAsync(
                    args.Message, "Unknown subject", args.Message.Subject, args.CancellationToken);
        }

        private Task OnErrorAsync(ProcessErrorEventArgs args)
        {
            logger.LogError(args.Exception, "ServiceBus error. Source={Source}, Entity={Entity}",
                args.ErrorSource, args.EntityPath);

            return Task.CompletedTask;
        }
    }
}
