using System;
using System.Collections.Generic;
using System.Text;

namespace Notifications.Worker.Common.Email
{
    public record EmailMessage(string To, string Subject, string Html, string? Text);
}
