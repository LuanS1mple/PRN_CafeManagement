using CafeManagent.dto.response.NotifyModuleDTO;
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
            if (!string.IsNullOrEmpty(staffRole))
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
            var context = _http.HttpContext;

            var staffRole = context.Session.GetString("StaffRole");
            var staffId = context.Session.GetInt32("StaffId");
            if ((staffId == null && staffRole == null) || _notify.IsViewded(staffId.Value))
            {
                return null;
            }
            else
            {
                _notify.AddToView(staffId.Value);
            }
            if (!string.IsNullOrEmpty(staffRole) && staffRole.Equals("Branch Manager"))
            {   
                return GetAllManager();
            }
            return GetAllStaff();
        }
        public void Clear()
        {
            var context = _http.HttpContext;
            var staffRole = context.Session.GetString("StaffRole");
            if (!string.IsNullOrEmpty(staffRole) && staffRole.Equals("Branch Manager"))
            {
                ClearManager();
            }

        }
        public List<Notify> GetAllStaff()
        {
            return _notify.AllStaff();
        }
        public List<Notify> GetAllManager()
        {
            return _notify.AllManager();
        }
        public void ClearManager()
        {
            _notify.ClearManager();
        }
    }
}
