using CafeManagent.dto.response;
using CafeManagent.Ulties;
using Microsoft.AspNetCore.SignalR;

namespace CafeManagent.Hubs
{
    public class NotifyHub : Hub
    {
        private readonly IHttpContextAccessor _http;
        private readonly NotifyUlti _notify;

        public NotifyHub(NotifyUlti notify, IHttpContextAccessor httpContextAccessor)
        {
            _notify = notify;
            this._http = httpContextAccessor;
        }
        public override async Task OnConnectedAsync()
        {
            var context = _http.HttpContext;

            var staffRole = context.Session.GetString("StaffRole");
            string group = "";
            if (string.IsNullOrEmpty(staffRole))
            {
                if (staffRole.Equals("Branch Manager"))
                {
                    group = "Manager";
                }
                else
                {
                    group = "Staff";
                }
            }
            else
            {
                group = "Manager";
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, group);

            await base.OnConnectedAsync();
        }
        public List<Notify> GetAll()
        {
            return _notify.All();
        }
        public void ClearAll()
        {
            _notify.Clear();
        }
    }
}
