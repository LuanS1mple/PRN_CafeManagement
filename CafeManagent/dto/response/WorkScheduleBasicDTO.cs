namespace CafeManagent.dto.response
{
    public class WorkScheduleBasicDTO
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public int WorkShiftId { get; set; }
        public DateOnly Date {  get; set; }
    }
}
