using JAT.Shared.Events;
using Notifications.Worker.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notifications.Worker.Features.Interviews
{
    public class InterviewCreatedEventHandler : IEventHandler<InterviewCreatedEvent>
    {
        public Task HandleAsync(InterviewCreatedEvent evt, CancellationToken ct)
        {
            Console.WriteLine($"Event '{typeof(InterviewCreatedEvent).Name}' has been successfully handled. Props: {evt.ApplicationTitle}, {evt.SheduledAt}");

            return Task.CompletedTask;
        }
    }
}
