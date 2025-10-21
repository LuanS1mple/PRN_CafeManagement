using System;
using System.Collections.Generic;

namespace CafeManagent.Models;

public partial class Staff
{
    public int StaffId { get; set; }

    public int? RoleId { get; set; }

    public string? FullName { get; set; }

    public bool? Gender { get; set; }

    public int? Status { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public DateTime? CreateAt { get; set; }

    public string? Img { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Contract? Contract { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Payroll> PayrollManagers { get; set; } = new List<Payroll>();

    public virtual ICollection<Payroll> PayrollStaffs { get; set; } = new List<Payroll>();

    public virtual ICollection<Request> RequestResolvedByNavigations { get; set; } = new List<Request>();

    public virtual ICollection<Request> RequestStaffs { get; set; } = new List<Request>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<Task> TaskManagers { get; set; } = new List<Task>();

    public virtual ICollection<Task> TaskStaffs { get; set; } = new List<Task>();

    public virtual ICollection<WorkSchedule> WorkScheduleManagers { get; set; } = new List<WorkSchedule>();

    public virtual ICollection<WorkSchedule> WorkScheduleStaffs { get; set; } = new List<WorkSchedule>();
}
