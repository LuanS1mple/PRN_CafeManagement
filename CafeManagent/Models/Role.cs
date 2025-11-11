using System;
using System.Collections.Generic;

namespace CafeManagent.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string? RoleName { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();

    public virtual ICollection<TaskType> TaskTypes { get; set; } = new List<TaskType>();
}
