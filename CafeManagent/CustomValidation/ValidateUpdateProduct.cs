using CafeManagent.dto.request.ProductModuleDTO;
using CafeManagent.Models;
using System.ComponentModel.DataAnnotations;

namespace CafeManagent.CustomValidation
{
    public class ValidateUpdateProduct : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            var dto = (UpdateProductDTO)context.ObjectInstance;
            var db = context.GetService<CafeManagementContext>();

            var product = db.Products.FirstOrDefault(p => p.ProductId == dto.ProductId);
            if (product == null)
                return new ValidationResult("Sản phẩm không tồn tại");

            if (db.Products.Any(p => p.ProductName == dto.ProductName && p.ProductId != dto.ProductId))
                return new ValidationResult("Tên sản phẩm đã tồn tại");

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
