using System;
using System.Collections.Generic;

namespace CafeManagent.Models;

public partial class Payroll
{
    public int PayrollId { get; set; }

    public int? StaffId { get; set; }

    public int? ManagerId { get; set; }

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }

    public decimal? TotalHour { get; set; }

    public decimal? OvertimeHour { get; set; }

    public decimal? Bonus { get; set; }

    public decimal? Penalty { get; set; }

    public decimal? TotalSalary { get; set; }

    public DateOnly? PaymentDate { get; set; }

    public int? Status { get; set; }

    public virtual Staff? Manager { get; set; }

    public virtual Staff? Staff { get; set; }
}
