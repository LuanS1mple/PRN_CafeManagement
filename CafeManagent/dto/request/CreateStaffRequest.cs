namespace CafeManagent.dto.request
{
    public class CreateStaffRequest
    {
        public int? RoleId { get; set; }
        public string? FullName { get; set; }
        public bool? Gender { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public IFormFile? AvatarFile { get; set; } // used in controller binding when form posts
    }

}
