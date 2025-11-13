using CafeManagent.dto.request;
using CafeManagent.dto.response;

namespace CafeManagent.Services
{
    public interface IWorkShiftService
    {
        Task<List<WorkShiftDTO>> GetAllWorkShiftsAsync();
        Task<List<WorkShiftDTO>> FilterWorkShiftsAsync(FilterWorkShiftDTO filter);
        Task<(bool Success, string Message)> AddWorkShiftAsync(AddWorkShiftDTO dto);
        Task<(bool Success, string Message)> DeleteWorkShiftAsync(int id);

    }
}
