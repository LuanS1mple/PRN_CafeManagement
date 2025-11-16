using CafeManagent.dto.request.WorkShiftModuleDTO;
using CafeManagent.Models;

namespace CafeManagent.mapper
{
    public class WorkShiftMapper
    {
        public static WorkSchedule FromAddWorkShiftDTO(AddWorkShiftDTO dto, int staffId, int workShiftId)
        {
            return new WorkSchedule
            {
                StaffId = staffId,
                Date = dto.Date,
                WorkshiftId = workShiftId,
                Description = dto.Note,
                ShiftName = dto.ShiftType,
                ManagerId =1,
            };
        }
    }
}
