using CafeManagent.Models;

namespace CafeManagent.Services
{
    public interface IWorkScheduleService
    {
        public List<WorkSchedule> GetWorkSchedulesToday();
    }
}
