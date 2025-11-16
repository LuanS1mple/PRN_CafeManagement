using System.ComponentModel.DataAnnotations;

namespace CafeManagent.dto.request.ProductModuleDTO
{
    public class IngredientItemDTO
    {
        [Required]
        public string IngredientName { get; set; }
        [Required]
        public string Unit { get; set; }
        [Required]
        public float QuantityNeeded { get; set; }
    }
}
