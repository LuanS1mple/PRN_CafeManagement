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
            [RegularExpression(@"^[^\d]+$", ErrorMessage = "Tên không được chứa số")]
             public string ProductName { get; set; }

            [Required]
            [Range(0.01, float.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
            public decimal Price { get; set; }

            [Required]
            [StringLength(500)]
            public string Description { get; set; }

            public List<IngredientItemDTO> Ingredients { get; set; } = new();
        }
    
}
