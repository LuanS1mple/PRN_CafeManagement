using System.ComponentModel.DataAnnotations;

namespace CafeManagent.dto.request.CustomerModuleDTO
{
    public class FilterCustomerProfileDTO
    {
        [Required]
        public string Keyword { get; set; }

    }
}
