using System;
using System.Collections.Generic;

namespace CafeManagent.Models;

public partial class RefreshToken
{
    public int Id { get; set; }

    public string? Token { get; set; }

    public DateTime? ExpireTime { get; set; }

    public bool? IsEnable { get; set; }

    public int? StaffId { get; set; }

    public virtual Staff? Staff { get; set; }
}
