using System.ComponentModel.DataAnnotations;

namespace CafeManagent.dto.request.CustomerModuleDTO
{
    public class UpdateCustomerProfileDTO
    {
        [Required]
        public int CustomerId { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public int LoyaltyPoint { get; set; }
    }
}
