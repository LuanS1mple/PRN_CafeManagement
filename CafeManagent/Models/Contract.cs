using System;
using System.Collections.Generic;

namespace CafeManagent.Models;

public partial class Contract
{
    public int ContractId { get; set; }

    public int? StaffId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public decimal? BaseSalary { get; set; }

    public string? Position { get; set; }

    public bool? Status { get; set; }

    public DateTime? SignedDate { get; set; }

    public string? Note { get; set; }

    public virtual Staff? Staff { get; set; }
}
