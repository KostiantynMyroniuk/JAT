using Notifications.Worker.Common.Email;
using Notifications.Worker.Infrastructure.Email.Senders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notifications.Worker.Infrastructure.Email
{
    public static class EmailExtensions
    {
        public static IServiceCollection AddEmailConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SmtpOptions>(configuration.GetSection("Email"));

            services.AddSingleton<IEmailSender, EmailSender>();

            return services;
        }
    }
}
