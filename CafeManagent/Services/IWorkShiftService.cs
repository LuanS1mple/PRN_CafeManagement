using CafeManagent.dto.request;
using CafeManagent.dto.response;

namespace CafeManagent.Services
{
    public interface IWorkShiftService
    {
        Task<(List<WorkShiftDTO> Shifts, int TotalItems)> GetPagedWorkShiftsAsync(int page, int pageSize);
        Task<(List<WorkShiftDTO> Shifts, int TotalItems)> FilterPagedWorkShiftsAsync(FilterWorkShiftDTO filter, int page, int pageSize);



        Task<(bool Success, string Message)> AddWorkShiftAsync(AddWorkShiftDTO dto);
        Task<(bool Success, string Message)> DeleteWorkShiftAsync(int id);

    }
}
