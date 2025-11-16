using CafeManagent.dto.request.ProductModuleDTO;
using CafeManagent.Models;

namespace CafeManagent.mapper
{
    public class ProductMapper
    {
        public static Product FromDTO(AddProductDTO dto)
        {
            return new Product
            {
                ProductName = dto.ProductName,
                Price = dto.Price,
                Description = dto.Description,
                CreatedAt = DateTime.Now,
                Status = true
            };
        }

        public static Ingredient FromDTO(IngredientItemDTO dto)
        {
            return new Ingredient
            {
                IngredientName = dto.IngredientName,
                Unit = dto.Unit,
                QuantityInStock = 0,
                Status = true,
                CreatedAt = DateTime.Now,
                CostPerUnit = 0
            };
        }

        public static Recipe FromDTO(Product product, Ingredient ingredient, float quantity)
        {
            return new Recipe
            {
                ProductId = product.ProductId,
                IngredientId = ingredient.IngredientId,
                QuantityNeeded = quantity
            };
        }

        public static void UpdateProductFromDTO(Product product, UpdateProductDTO dto)
        {
            product.ProductName = dto.ProductName;
            product.Price = dto.Price;
            product.Description = dto.Description;
        }


    }
}
