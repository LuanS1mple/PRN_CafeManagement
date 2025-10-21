using Microsoft.AspNetCore.Mvc;

#nullable enable
using System;

namespace CafeManagent.dto.request;

public class UpdateStaffProfile
{
    public int StaffId { get; set; }
   
    public string? FullName { get; set; }
    public bool? Gender { get; set; }          // true: Nam, false: Nữ
    public DateOnly? BirthDate { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public DateTime? CreateAt { get; set; }
    public string? Password { get; set; }      // để trống nếu không đổi
}


