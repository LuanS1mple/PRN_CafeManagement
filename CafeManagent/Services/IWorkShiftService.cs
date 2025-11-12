using CafeManagent.dto.request;
using CafeManagent.Models;

namespace CafeManagent.Services
{
    public interface IWorkShiftService
    {
        public List<WorkSchedules> GetAll();
        public List<WorkSchedules> Filter(FilterWorkShiftDTO filter);
        public bool AddWorkShift(AddWorkShiftDTO dto, out string message);
        public bool DeleteWorkShift(int id, out string message);

        public List<string> GetPositions();
        public List<string> GetShiftTypes();
        public List<string> GetEmployees();

        public int GetTotalShifts(List<object> shifts);
        public int GetTotalEmployees(List<object> shifts);
        public int GetTodayShifts(List<object> shifts);
        public double GetTotalHours(List<object> shifts);
    }
}
