using CafeManagent.Models;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Services
{
    public interface IWorkScheduleService
    {
        public WorkSchedule Get(int staffId, int workShiftId, DateOnly date);
        public List<WorkSchedule> Get(int staffId);
        public WorkSchedule GetById(int id);
    }
}
