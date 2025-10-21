using System;
using System.Collections.Generic;

namespace CafeManagent.Models;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int? StaffId { get; set; }

    public int? ShiftId { get; set; }

    public int? WorkshiftId { get; set; }

    public TimeOnly? CheckIn { get; set; }

    public TimeOnly? CheckOut { get; set; }

    public DateOnly? Workdate { get; set; }

    public int? Status { get; set; }

    public decimal? TotalHour { get; set; }

    public string? Note { get; set; }

    public virtual WorkSchedule? Shift { get; set; }

    public virtual Staff? Staff { get; set; }

    public virtual WorkShift? Workshift { get; set; }
}
