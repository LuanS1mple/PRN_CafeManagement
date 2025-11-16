using CafeManagent.dto.request.ProductModuleDTO;
using CafeManagent.Models;
using System.ComponentModel.DataAnnotations;

namespace CafeManagent.CustomValidation
{
    public class ValidateAddProduct : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            var dto = (AddProductDTO)context.ObjectInstance;
            var db = (CafeManagementContext)context.GetService(typeof(CafeManagementContext));

            if (string.IsNullOrWhiteSpace(dto.ProductName))
                return new ValidationResult("Tên sản phẩm không được để trống");

            if (db.Products.Any(p => p.ProductName == dto.ProductName))
                return new ValidationResult("Sản phẩm đã tồn tại");

            if (dto.Price < 0)
                return new ValidationResult("Giá sản phẩm phải >= 0");

            if (dto.Ingredients != null)
            {
                foreach (var ing in dto.Ingredients)
                {
                    if (string.IsNullOrWhiteSpace(ing.IngredientName) ||
                        string.IsNullOrWhiteSpace(ing.Unit) ||
                        ing.QuantityNeeded <= 0)
                        return new ValidationResult("Nguyên liệu không hợp lệ");
                }
            }

            return ValidationResult.Success;
        }
    }
}
