using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Notifications.Worker.Infrastructure.Email
{
    public class SmtpOptions
    {
        public required string Host { get; init; }
        public required int Port { get; init; } = 587;

        [EmailAddress]
        public required string FromEmail { get; init; }
        public string? FromName { get; init; }
        public required string AppPassword { get; init; }
    }
}
