using System;
using System.Collections.Generic;

namespace CafeManagent.Models;

public partial class Request
{
    public int ReportId { get; set; }

    public int? StaffId { get; set; }

    public int? ResolvedBy { get; set; }

    public DateTime? ReportDate { get; set; }

    public string? ReportType { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public int? Status { get; set; }

    public string? Detail { get; set; }

    public DateTime? ResolvedDate { get; set; }

    public virtual Staff? ResolvedByNavigation { get; set; }

    public virtual Staff? Staff { get; set; }
}
