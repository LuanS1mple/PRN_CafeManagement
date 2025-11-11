using System;
using System.Collections.Generic;

namespace CafeManagent.Models;

public partial class Ingredient
{
    public int IngredientId { get; set; }

    public string? IngredientName { get; set; }

    public string? Unit { get; set; }

    public double? QuantityInStock { get; set; }

    public double? CostPerUnit { get; set; }

    public bool? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}
