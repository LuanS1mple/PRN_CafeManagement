using CafeManagent.Hubs;
using CafeManagent.Ulties;
using Microsoft.AspNetCore.SignalR;

namespace CafeManagent.Middlewares
{
    public class NotifyMiddleware
    {
        private readonly RequestDelegate _next;

        public NotifyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context,
                                 NotifyUlti ulti,
                                 IHubContext<NotifyHub> hub)
        {
            await _next(context);

            var list = ulti.All();

            if (list.Count > 0)
            {
                await hub.Clients.All
                    .SendAsync("ReceiveNotify", list);
            }
        }
    }
}
