using System.ComponentModel.DataAnnotations;

namespace CafeManagent.dto.request.ProductModuleDTO
{
    public class IngredientItemDTO
    {
        [Required]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Tên nguyên liệu được chứa số")]
        public string IngredientName { get; set; }
        [Required]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Tên định lượng không được chứa số")]
        public string Unit { get; set; }
        [Required]
        [Range(0.01, 100, ErrorMessage = "Định lượng phải lớn hơn 0")]
        public float QuantityNeeded { get; set; }
    }
}
