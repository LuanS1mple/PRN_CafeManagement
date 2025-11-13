namespace CafeManagent.dto.Task
{
    public class TaskReportDTO
    {
        public int TaskId { get; set; }
        public string TasktypeName { get; set; }
        public string Description { get; set; }
        public string StaffName { get; set; }
        public string ManagerName { get; set; }
        public DateTime? AssignTime { get; set; }
        public DateTime? DueTime { get; set; }
        public int? Status { get; set; }
    }
}
