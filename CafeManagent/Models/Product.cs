using System;
using System.Collections.Generic;

namespace CafeManagent.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public decimal? Price { get; set; }

    public string? ProductImage { get; set; }

    public bool? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Description { get; set; }
}
