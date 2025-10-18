using System;
using System.Collections.Generic;

namespace CafeManagent.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public int? LoyaltyPoint { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
