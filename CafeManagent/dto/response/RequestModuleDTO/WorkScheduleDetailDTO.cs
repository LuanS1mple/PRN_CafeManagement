namespace CafeManagent.dto.response.RequestModuleDTO
{
    public class WorkScheduleDetailDTO
    {
        public int Id { get; set; }
        public int ShiftId { get; set; }
        public DateOnly Date { get; set; }
        public string Description { get; set; }
        public string ManagerName { get; set; }
        public string StaffName { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
