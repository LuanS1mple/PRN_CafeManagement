using System;
using System.Collections.Generic;

namespace CafeManagent.Models;

public partial class Recipe
{
    public int RecipeId { get; set; }

    public int? ProductId { get; set; }

    public int? IngredientId { get; set; }

    public double? QuantityNeeded { get; set; }

    public virtual Ingredient? Ingredient { get; set; }

    public virtual Product? Product { get; set; }
}
