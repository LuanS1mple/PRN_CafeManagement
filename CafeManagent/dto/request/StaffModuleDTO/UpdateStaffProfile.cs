using System.ComponentModel.DataAnnotations;

namespace CafeManagent.dto.request.StaffModuleDTO
{
    public class UpdateStaffProfile
    {
        [Required]
        public int StaffId { get; set; }

        public int? RoleId { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự")]
        public string? FullName { get; set; }

        public bool? Gender { get; set; }       

        public DateOnly? BirthDate { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "SĐT là bắt buộc")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "SĐT phải gồm đúng 9 chữ số")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(200)]
        public string? Email { get; set; }

        // ------- Contract fields to update -------
        [Required(ErrorMessage = "Chức danh (trong HĐ) là bắt buộc")]
        [StringLength(100, ErrorMessage = "Chức danh tối đa 100 ký tự")]
        public string? Position { get; set; }

        [Required(ErrorMessage = "Ngày hết hạn HĐ là bắt buộc")]
        public DateOnly? ContractEndDate { get; set; }
    }
}
