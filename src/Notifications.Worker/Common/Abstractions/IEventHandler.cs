using System;
using System.Collections.Generic;
using System.Text;

namespace Notifications.Worker.Common.Abstractions
{
    public interface IEventHandler<in TEvent>
    {
        Task HandleAsync(TEvent evt, CancellationToken ct);
    }
}
