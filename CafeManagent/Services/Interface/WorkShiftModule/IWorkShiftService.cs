using CafeManagent.dto.request.WorkShiftModuleDTO;
using CafeManagent.dto.response.WorkShiftDTO;

namespace CafeManagent.Services.Interface.WorkShiftModule
{
    public interface IWorkShiftService
    {
        Task<(List<WorkShiftDTO> shifts, int totalItems)> GetPagedWorkShiftsAsync(int page, int pageSize);
        Task<(List<WorkShiftDTO> shifts, int totalItems)> FilterPagedWorkShiftsAsync(FilterWorkShiftDTO filter, int page, int pageSize);

        Task<(bool Success, CafeManagent.Enums.NotifyMessage Notify)> AddWorkShiftAsync(AddWorkShiftDTO dto);
        Task<(bool Success, CafeManagent.Enums.NotifyMessage Notify)> UpdateWorkShiftAsync(UpdateWorkShiftDTO dto);
        Task<(bool Success, CafeManagent.Enums.NotifyMessage Notify)> DeleteWorkShiftAsync(int id);
    }


}
