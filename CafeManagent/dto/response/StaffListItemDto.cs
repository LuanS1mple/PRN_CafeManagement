namespace CafeManagent.dto.response
{
    public record StaffListItemDto(
        int StaffId,
        string FullName,
        string? Email,
        string? Title,             // từ Contract.Position
        string Status,             // map từ Staff.Status
        DateOnly? JoinDate,        // từ Contract.StartDate
        DateOnly? ContractEndDate, // từ Contract.EndDate
        string AvatarUrl
    );
}
