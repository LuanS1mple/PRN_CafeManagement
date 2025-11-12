using CafeManagent.dto.request;

namespace CafeManagent.Services
{
    public interface IWorkShiftService
    {
        Task<(List<object> Shifts, int TotalItems)> GetPagedAsync(int page, int pageSize);
        Task<(List<object> Shifts, int TotalItems)> FilterAsync(FilterWorkShiftDTO filter, int page, int pageSize);
        Task<(bool Success, string Message)> AddAsync(AddWorkShiftDTO dto);
        Task<(bool Success, string Message)> UpdateAsync(UpdateWorkShiftDTO dto);
        Task<(bool Success, string Message)> DeleteAsync(int id);
        Task<(List<string> Positions, List<string> ShiftTypes, List<string> Employees)> GetFilterDataAsync();
    }
}
