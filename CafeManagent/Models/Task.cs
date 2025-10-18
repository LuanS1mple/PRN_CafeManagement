using System;
using System.Collections.Generic;

namespace CafeManagent.Models;

public partial class Task
{
    public int TaskId { get; set; }

    public int? StaffId { get; set; }

    public int? ManagerId { get; set; }

    public string? TaskName { get; set; }

    public string? Description { get; set; }

    public DateTime? AssignTime { get; set; }

    public DateTime? DueTime { get; set; }

    public int? Status { get; set; }

    public virtual Staff? Manager { get; set; }

    public virtual Staff? Staff { get; set; }
}
