    namespace CafeManagent.dto.request
{
    public class AttendanceRequestDTO
    {
       public int StaffId { get; set; }
        public DateOnly WorkDate { get; set; }
        public int WorkShiftId { get; set; }
        public TimeOnly UpdateCheckIn { get; set; }
        public TimeOnly UpdateCheckOut { get; set; }
        public string Type { get; set; }
        public string Reason { get; set; }
    }
}
