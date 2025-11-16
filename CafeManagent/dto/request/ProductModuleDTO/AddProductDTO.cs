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
        public decimal Price { get; set; }

        [Required]
        public string Description { get; set; }

        public List<IngredientItemDTO> Ingredients { get; set; } = new();
    }

    
}
