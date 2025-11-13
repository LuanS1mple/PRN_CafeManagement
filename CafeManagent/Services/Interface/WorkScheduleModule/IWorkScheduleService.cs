using CafeManagent.Models;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Services.Interface.WorkScheduleModule
{
    public interface IWorkScheduleService
    {
        public WorkSchedule Get(int staffId, int workShiftId, DateOnly date);
        public List<WorkSchedule> Get(int staffId);
        public WorkSchedule GetById(int id);
        public void Save(WorkSchedule workSchedule);
        public void Update(WorkSchedule workSchedule);
        public void Delete(WorkSchedule workSchedule);
        public List<WorkSchedule> GetWorkSchedulesToday();
    }
}
