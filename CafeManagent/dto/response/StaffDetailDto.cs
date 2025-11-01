namespace CafeManagent.dto.response
{
    public record StaffDetailDto(
        int StaffId,
        string? FullName,
        string? Email,
        string? Title,             // Contract.Position
        string Status,             // map từ Staff.Status
        DateOnly? BirthDate,
        string? Phone,
        string? Address,
        DateOnly? JoinDate,        // Contract.StartDate
        DateOnly? ContractEndDate, // Contract.EndDate
        string AvatarUrl,
        string? RoleName           // Role.RoleName
    );
}
