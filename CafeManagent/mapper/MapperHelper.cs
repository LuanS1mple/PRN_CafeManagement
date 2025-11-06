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
                ShiftId = w.WorkshiftId!.Value,
                Date = w.Date ?? default,
                Description = w.Description ?? "",
                ManagerName = w.Manager?.FullName ?? "",
                StaffName = w.Staff?.FullName ?? "",
                StartTime = w.Workshift?.StartTime ?? default,
                EndTime = w.Workshift?.EndTime ?? default
            };
        }
        public static Request FromWorkScheduleRequestDTO(WorkScheduleRequestDTO workScheduleRequestDTO,int staffId)
        {
            string detail = "";
            if (!workScheduleRequestDTO.Require.Equals("change"))
            {
                detail = workScheduleRequestDTO.DateChange.ToString() + ";" + workScheduleRequestDTO.OldShiftId + ";" + "Cancel";
            }
            else
            {
                detail = workScheduleRequestDTO.DateChange.ToString() + ";"+workScheduleRequestDTO.OldShiftId + ";"+workScheduleRequestDTO.NewShiftId;
            }
            return new Request()
            {
                StaffId = staffId,
                ReportDate = DateTime.Now,
                ReportType = "WorkSchedule",
                Title = workScheduleRequestDTO.Title,
                Description = workScheduleRequestDTO.Description,
                Status = 0,
                Detail = detail
            };
        }

    }
}
