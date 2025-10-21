using CafeManagent.Models;

namespace CafeManagent.Services
{
    public interface IStaffService
    {
        public Staff? Authentication(string username, string password);
    }
}
