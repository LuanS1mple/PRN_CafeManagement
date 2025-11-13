namespace CafeManagent.dto.response
{
    public class StaffListQuery
    {
        public string? Q { get; set; }       // search name/email/code
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 5;
        public int? Status { get; set; }     // lọc theo mã Status nếu muốn
    }
}
