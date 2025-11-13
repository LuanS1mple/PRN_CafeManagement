using CafeManagent.dto.response.NotifyModuleDTO;
using Microsoft.AspNetCore.SignalR;

namespace CafeManagent.Hubs
{
    public class ResponseHub : Hub
    {
        public static Dictionary<int, SystemNotify?> Notifies { get; }
            = new Dictionary<int, SystemNotify?>();

        private static readonly object _lock = new object();
        public static void SetNotify(int userId, SystemNotify notify)
        {
            lock (_lock)
            {
                Notifies[userId] = notify;
            }
        }

        public async Task GetCurrentNotify()
        {
            int? userId = Context.GetHttpContext()!
                 .Session.GetInt32("StaffId");

            if (userId == null)
            {
                return;
            }
            SystemNotify? notify;

            lock (_lock)
            {
                Notifies.TryGetValue(userId.Value, out notify);
                Notifies[userId.Value] = null; 
            }

            if (notify == null)
            {
                await Clients.Caller.SendAsync("ReceiveResponseStatus", null, null);
                return;
            }

            await Clients.Caller.SendAsync(
                "ReceiveResponseStatus",
                notify.IsSuccess,
                notify.Message
            );
        }
    }
}
