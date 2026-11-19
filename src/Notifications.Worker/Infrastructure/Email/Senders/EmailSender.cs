using MailKit;
using Microsoft.Extensions.Options;
using MimeKit;
using Notifications.Worker.Common.Email;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Notifications.Worker.Infrastructure.Email.Senders
{
    public class EmailSender(
        IOptions<SmtpOptions> options,
        ILogger<EmailSender> logger) : IEmailSender
    {
        public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
        {
            var opt = options.Value;

            var mime = new MimeMessage() { Subject = message.Subject };
            mime.From.Add(new MailboxAddress(opt.FromName, opt.FromEmail));
            mime.To.Add(MailboxAddress.Parse(message.To));
            mime.Body = new BodyBuilder
            {
                HtmlBody = message.Html,
                TextBody = message.Text
            }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(opt.Host, opt.Port, cancellationToken: ct);

            if (!string.IsNullOrEmpty(opt.AppPassword))
                await client.AuthenticateAsync(opt.FromEmail, opt.AppPassword, ct);

            await client.SendAsync(mime, ct);
            await client.DisconnectAsync(quit: true, ct);

            logger.LogInformation("Email sent to: '{To}'", message.To);
        }
    }
}
