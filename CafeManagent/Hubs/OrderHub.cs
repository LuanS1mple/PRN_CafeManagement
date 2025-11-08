using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CafeManagent.Hubs
{
    public class OrderHub : Hub
    {
        
        public async Task SendNewOrder(object order)
        {
            await Clients.All.SendAsync("ReceiveNewOrder", order);
        }

        public async Task SendOrderCanceled(int orderId)
        {
            await Clients.All.SendAsync("OrderCanceled", orderId);
        }
        public async Task SendOrderStatusUpdate(int orderId, int newStatus, string newStatusText)
        {
            await Clients.All.SendAsync("ReceiveOrderStatusUpdate", orderId, newStatus, newStatusText);
        }
    }
}


