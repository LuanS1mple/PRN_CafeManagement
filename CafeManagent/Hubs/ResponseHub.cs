using CafeManagent.dto.response;
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
            int userId = int.Parse(Context.GetHttpContext()!
                .Session.Get("StaffId"));
        
            //int userId = 1;

            SystemNotify? notify;

            lock (_lock)
            {
                Notifies.TryGetValue(userId, out notify);
                Notifies[userId] = null; 
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
