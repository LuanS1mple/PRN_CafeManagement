using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CafeManagent.dto.request
{
    public class CreateStaffRequest
    {
        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public int RoleId { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public bool? Gender { get; set; }
        public DateOnly? BirthDate { get; set; }

        [Required, StringLength(255)]
        public string Address { get; set; } = string.Empty;

        [Required, RegularExpression(@"^\d{9}$", ErrorMessage = "SĐT phải gồm đúng 9 chữ số")]
        public string Phone { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string UserName { get; set; } = string.Empty;

        // 👉 Position nhập riêng cho Hợp đồng, KHÔNG liên quan Role
        [Required(ErrorMessage = "Chức danh (trong HĐ) là bắt buộc")]
        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Tiền lương không được âm")]
        public decimal BaseSalary { get; set; }

        public IFormFile? AvatarFile { get; set; }
    }
}
