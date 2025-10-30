using Microsoft.AspNetCore.SignalR;

namespace CafeManagent.Hubs
{
    public class ResponseHub : Hub
    {
        public async Task ResponseStatus(bool isSuccess,string msg)
        {
            await Clients.Caller.SendAsync("ReceiveResponseStatus",isSuccess,msg);
        }
    }
}
