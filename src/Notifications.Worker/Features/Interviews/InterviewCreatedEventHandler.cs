using JAT.Shared.Events;
using Notifications.Worker.Common.Email;
using Notifications.Worker.Common.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notifications.Worker.Features.Interviews
{
    public class InterviewCreatedEventHandler(
        IEmailSender emailSender) : IEventHandler<InterviewCreatedEvent>
    {
        public async Task HandleAsync(InterviewCreatedEvent evt, CancellationToken ct)
        {
            var message = new EmailMessage(
                evt.To,
                "Interview created",
                $"<h1> Interview for {evt.ApplicationTitle} created",
                null);

            await emailSender.SendAsync(message, ct);
        }
    }
}
