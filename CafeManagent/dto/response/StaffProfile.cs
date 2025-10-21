using Microsoft.AspNetCore.Mvc;
#nullable enable
using System;

namespace CafeManagent.dto.response;

public record StaffProfile(
    int StaffId,
    int? RoleId,
    string? RoleName,
    string? FullName,
    bool? Gender,
    DateOnly? BirthDate,
    string? Address,
    string? Phone,
    string? Email,
    string? UserName,
    DateTime? CreateAt,
    string? Img
);
