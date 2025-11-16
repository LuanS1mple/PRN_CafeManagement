using System.ComponentModel.DataAnnotations;
using CafeManagent.CustomValidation;

namespace CafeManagent.dto.request.ProductModuleDTO
{

        [ValidateUpdateProduct] // Custom validation giống UpdateWorkShift
        public class UpdateProductDTO
        {
            [Required]
            public int ProductId { get; set; }

            [Required]
            public string ProductName { get; set; }

            [Required]
            public decimal Price { get; set; }

            [Required]
            public string Description { get; set; }

            public List<IngredientItemDTO> Ingredients { get; set; } = new();
        }
    
}
