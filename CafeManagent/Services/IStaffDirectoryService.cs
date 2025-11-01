using CafeManagent.dto.response;

namespace CafeManagent.Services
{
    public interface IStaffDirectoryService
    {
        Task<PagedResult<StaffListItemDto>> GetPagedAsync(StaffListQuery q, CancellationToken ct = default);
        Task<StaffDetailDto?> GetDetailAsync(int staffId, CancellationToken ct = default);
    }
}
