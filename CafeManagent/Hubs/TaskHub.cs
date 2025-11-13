using Microsoft.AspNetCore.SignalR;

namespace CafeManagent.Hubs
{
    public class TaskHub : Hub
    {
        public async Task SendUpdateTaskNotification (string message)
        {
            await Clients.All.SendAsync("ReceiveTaskUpdate", message);
        }
    }
}
