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
            string role = context.Session.GetString("StaffRole");
            if (!string.IsNullOrEmpty(role))
            {
                if (!role.Equals("Branch Manager"))
                {
                    var list = ulti.AllManager();
                    await hub.Clients.Group("Manager")
                   .SendAsync("ReceiveNotify", list);
                }
                else
                {
                    var list = ulti.AllStaff();
                    await hub.Clients.Group("Staff")
                   .SendAsync("ReceiveNotify", list);
                }
            }

        }
    }
}
