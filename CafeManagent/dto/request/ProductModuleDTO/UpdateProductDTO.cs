using System.ComponentModel.DataAnnotations;
using CafeManagent.CustomValidation;

namespace CafeManagent.dto.request.ProductModuleDTO
{

        [ValidateUpdateProduct] 
        public class UpdateProductDTO
        {
            [Required]
            public int ProductId { get; set; }

            [Required]
            [RegularExpression(@"^[^\d]+$", ErrorMessage = "Không được chứa số")]
             public string ProductName { get; set; }

            [Required]
            [Range(0.01, float.MaxValue, ErrorMessage = "Phải lớn hơn 0")]
            public decimal Price { get; set; }

            [Required]
            [RegularExpression(@"^[^\d]+$", ErrorMessage = "Không được chứa số")]
            public string Description { get; set; }

            public List<IngredientItemDTO> Ingredients { get; set; } = new();
        }
    
}
