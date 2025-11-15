using CafeManagent.dto.response.StaffWorkScheduleDTO;
using CafeManagent.dto.response.WorkShiftDTO;

namespace CafeManagent.Services.Interface.StaffWorkScheduleModule
{
    public interface IStaffWorkScheduleService
    {
        Task<List<StaffWorkScheduleDTO>> GetWorkShiftsByStaffAsync(int staffId);
    }
}
