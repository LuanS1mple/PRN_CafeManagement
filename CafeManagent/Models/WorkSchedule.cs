using System;
using System.Collections.Generic;

namespace CafeManagent.Models;

public partial class WorkSchedule
{
    public int ShiftId { get; set; }

    public int? WorkshiftId { get; set; }

    public int? ManagerId { get; set; }

    public int? StaffId { get; set; }

    public string? ShiftName { get; set; }

    public DateOnly? Date { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Staff? Manager { get; set; }

    public virtual Staff? Staff { get; set; }

    public virtual WorkShift? Workshift { get; set; }
}
