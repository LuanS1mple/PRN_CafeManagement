using Microsoft.AspNetCore.SignalR;

namespace CafeManagent.Hubs
{
    public class WorkShiftHub : Hub
    {
        // Chỉ gửi tới tất cả client đang mở trang
        public async Task SendWorkShiftUpdate(object newShift)
        {
            await Clients.All.SendAsync("ReceiveWorkShiftUpdate", newShift);
        }
    }
}
