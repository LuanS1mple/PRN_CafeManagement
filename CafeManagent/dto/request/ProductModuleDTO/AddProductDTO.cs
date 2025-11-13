namespace CafeManagent.dto.request.ProductModuleDTO
{
    public class AddProductDTO
    {
        public string ProductName { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }

        public string IngredientName { get; set; }

        public string Unit { get; set; }

        public float QuantityNeeded { get; set; }

    }
}
