using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CafeManagent.Hubs
{
    public class OrderHub : Hub
    {
        // Gửi thông báo tới tất cả client bartender
        public async Task SendNewOrder(object order)
        {
            await Clients.All.SendAsync("ReceiveNewOrder", order);
        }

        public async Task SendOrderCanceled(int orderId)
        {
            await Clients.All.SendAsync("OrderCanceled", orderId);
        }
    }
}


