using CafeManagent.dto.request;
using CafeManagent.dto.response;
using CafeManagent.Models;

namespace CafeManagent.mapper
{
    public class MapperHelper
    {
        public static Request GetRequestFromAttendanceRequestDTO(AttendanceRequestDTO attendanceRequestDTO)
        {
            return new Request
            {
                StaffId = attendanceRequestDTO.StaffId,
                ReportDate = DateTime.Now,
                ReportType = "Attendance",
                Title = "Chỉnh sửa điểm danh",
                Description = attendanceRequestDTO.Reason,
                Status = 0,
                Detail = attendanceRequestDTO.WorkDate.ToString() + ";" + attendanceRequestDTO.WorkShiftId + ";" + attendanceRequestDTO.UpdateCheckIn.ToString() + ";" + attendanceRequestDTO.UpdateCheckOut,

            };
        }
        public static List<WorkScheduleBasicDTO> FromWorkSchedule(List<WorkSchedule> raw)
        {
            return raw.Select(raw => new WorkScheduleBasicDTO {
                Id= raw.ShiftId,
                StaffId = raw.StaffId!.Value,
                Date = raw.Date!.Value,
                WorkShiftId = raw.WorkshiftId!.Value
            }).ToList();
        }
        public static WorkScheduleDetailDTO FromWorkSchedule(WorkSchedule w)
        {
            if (w == null)
                return null;

            return new WorkScheduleDetailDTO
            {
                Id = w.ShiftId, 
                ShiftName = w.ShiftName ?? w.Workshift?.ShiftName ?? "",
                Date = w.Date ?? default,
                Description = w.Description ?? "",
                ManagerName = w.Manager?.FullName ?? "",
                StaffName = w.Staff?.FullName ?? "",
                StartTime = w.Workshift?.StartTime ?? default,
                EndTime = w.Workshift?.EndTime ?? default
            };
        }

    }
}
