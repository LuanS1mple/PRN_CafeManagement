using System.ComponentModel.DataAnnotations;
using CafeManagent.CustomValidation;

namespace CafeManagent.dto.request.ProductModuleDTO
{
    [ValidateAddProduct] 
    public class AddProductDTO
    {
        [Required]
        public string ProductName { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        public List<IngredientItemDTO> Ingredients { get; set; } = new();
    }

    
}
