using System;
using System.Collections.Generic;
using System.Text;

namespace JAT.Shared.Events
{
    public record InterviewCreatedEvent(string ApplicationTitle, DateTimeOffset SheduledAt);
}
