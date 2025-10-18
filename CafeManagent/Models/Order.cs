using System;
using System.Collections.Generic;

namespace CafeManagent.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? StaffId { get; set; }

    public int? CustomerId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public int? Status { get; set; }

    public string? Note { get; set; }

    public decimal? Discount { get; set; }

    public bool? Vat { get; set; }

    public decimal? OrderPrice { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Staff? Staff { get; set; }
}
