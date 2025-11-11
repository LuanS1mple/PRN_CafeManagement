using System;
using System.Collections.Generic;

namespace CafeManagent.Models;

public partial class TaskType
{
    public int TasktypeId { get; set; }

    public string? TaskName { get; set; }

    public string? Description { get; set; }

    public int? RoleId { get; set; }

    public virtual Role? Role { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
