using System.ComponentModel.DataAnnotations;

namespace CafeManagent.dto.request.ProductModuleDTO
{
    public class IngredientItemDTO
    {
        [Required]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Không được chứa số")]
        public string IngredientName { get; set; }
        [Required]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Không được chứa số")]
        public string Unit { get; set; }
        [Required]
        [Range(0.01, 100, ErrorMessage = "Phải lớn hơn 0")]
        public float QuantityNeeded { get; set; }
    }
}
