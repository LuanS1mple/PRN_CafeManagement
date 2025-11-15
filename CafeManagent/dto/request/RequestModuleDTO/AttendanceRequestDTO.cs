using CafeManagent.CustomValidation;
using System.ComponentModel.DataAnnotations;

namespace CafeManagent.dto.request.RequestModuleDTO
{
    public class AttendanceRequestDTO
    {
        [Required]
        public int StaffId { get; set; }
        [Required]
        public DateOnly WorkDate { get; set; }
        [Required]
        public int WorkShiftId { get; set; }
        [Required]
        [CheckInBeforeCheckOut("UpdateCheckOut","Check in phải trước check out")]
        public TimeOnly UpdateCheckIn { get; set; }
        [Required]
        public TimeOnly UpdateCheckOut { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        [StringLength(500)]
        public string Reason { get; set; }
    }
}
