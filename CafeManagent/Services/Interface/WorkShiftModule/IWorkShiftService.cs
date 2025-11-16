using CafeManagent.dto.request.WorkShiftModuleDTO;
using CafeManagent.dto.response.WorkShiftDTO;
using CafeManagent.Models;

namespace CafeManagent.Services.Interface.WorkShiftModule
{
    public interface IWorkShiftService
    {
        Task<(List<WorkShiftDTO> shifts, int totalItems)> GetPagedWorkShiftsAsync(int page, int pageSize);
        Task<(List<WorkShiftDTO> shifts, int totalItems)> FilterPagedWorkShiftsAsync(FilterWorkShiftDTO filter, int page, int pageSize);

        Task<bool> AddWorkShiftAsync(AddWorkShiftDTO dto);
        Task<bool> UpdateWorkShiftAsync(UpdateWorkShiftDTO dto);
        Task<bool> DeleteWorkShiftAsync(int id);
    }


}
