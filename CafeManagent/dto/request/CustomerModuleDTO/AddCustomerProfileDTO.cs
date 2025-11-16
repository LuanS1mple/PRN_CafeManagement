using System.ComponentModel.DataAnnotations;

namespace CafeManagent.dto.request.CustomerModuleDTO
{
    public class AddCustomerProfileDTO
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }
        [Required]
        [StringLength(200)]
        public string Address { get; set; }
        [Required]
        [Range(1, 10000, ErrorMessage = "Điểm phải lớn hơn 0")]
        public int LoyaltyPoint { get; set; }
    }
}
