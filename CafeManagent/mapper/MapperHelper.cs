using CafeManagent.dto.request;
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
    }
}
