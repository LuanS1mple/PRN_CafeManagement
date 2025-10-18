using System;
using System.Collections.Generic;

namespace CafeManagent.Models;

public partial class OrderItem
{
    public int ItemId { get; set; }

    public int? OrderId { get; set; }

    public string? ProductName { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public virtual Order? Order { get; set; }
}
