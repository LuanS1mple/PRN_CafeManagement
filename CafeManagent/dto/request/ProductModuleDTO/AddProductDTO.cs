using System.ComponentModel.DataAnnotations;

namespace CafeManagent.dto.request.ProductModuleDTO
{
    public class AddProductDTO
    {
        [Required]
        public string ProductName { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public string Description { get; set; }

        public List<IngredientItemDTO> Ingredients { get; set; }

    }
}
