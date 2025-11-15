using System.ComponentModel.DataAnnotations;

namespace CafeManagent.dto.request.StaffModuleDTO
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Bạn cần nhập mật khẩu hiện tại")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải ít nhất 6 ký tự")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public int StaffId { get; set; }
    }
}
