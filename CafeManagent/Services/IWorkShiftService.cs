using CafeManagent.dto.request;
using CafeManagent.Models;

namespace CafeManagent.Services
{
    public interface IWorkShiftService
    {
        List<WorkShift> GetPaged(int page, int pageSize, out int totalItems);
        List<WorkShift> Filter(FilterWorkShiftDTO filter, int page, int pageSize, out int totalItems);
        void Add(AddWorkShiftDTO dto, out bool success, out string message);
        void Update(UpdateWorkShiftDTO dto, out bool success, out string message);
        void Delete(int id, out bool success, out string message);
        void GetFilterData(out List<string> positions, out List<string> shiftTypes, out List<string> employees);
    }
}
