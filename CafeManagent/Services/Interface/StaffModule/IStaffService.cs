using CafeManagent.Models;

namespace CafeManagent.Services.Interface.StaffModule
{
    public interface IStaffService
    {
        public Staff? Authentication(string username, string password);
    }
}
