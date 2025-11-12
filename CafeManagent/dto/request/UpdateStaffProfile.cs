using System.ComponentModel.DataAnnotations;

namespace CafeManagent.dto.request
{
    public class UpdateStaffProfile
    {
        [Required]
        public int StaffId { get; set; }

        public int? RoleId { get; set; }

        [Required, StringLength(100)]
        public string? FullName { get; set; }

        public bool? Gender { get; set; }          // true: Nam, false: Nữ

        public DateOnly? BirthDate { get; set; }

        [Required, StringLength(255)]
        public string? Address { get; set; }

        [Required, RegularExpression(@"^\d{9}$", ErrorMessage = "SĐT phải gồm đúng 9 chữ số")]
        public string? Phone { get; set; }

        [Required, EmailAddress, StringLength(200)]
        public string? Email { get; set; }

        // ------- Contract fields to update -------
        [Required(ErrorMessage = "Chức danh (trong HĐ) là bắt buộc")]
        [StringLength(100)]
        public string? Position { get; set; }

        [Required(ErrorMessage = "Ngày hết hạn HĐ là bắt buộc")]
        public DateOnly? ContractEndDate { get; set; }
    }
}
