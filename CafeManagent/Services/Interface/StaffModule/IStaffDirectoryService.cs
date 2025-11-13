using CafeManagent.dto.request.StaffModuleDTO;
using CafeManagent.dto.response;
using CafeManagent.dto.response.StaffModuleDTO;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CafeManagent.Services.Interface.StaffModule
{
    public interface IStaffDirectoryService
    {
        Task<PagedResult<StaffListItemDto>> GetPagedAsync(StaffListQuery q, CancellationToken ct = default);
        Task<StaffDetailDto?> GetDetailAsync(int staffId, CancellationToken ct = default);
        Task<bool> UpdateAsync(UpdateStaffProfile dto, IFormFile? avatarFile, string webRootPath, CancellationToken ct = default);
        Task<int> CreateAsync(CreateStaffRequest req, string webRootPath, CancellationToken ct = default);
        Task<List<SelectListItem>> GetRolesSelectListAsync(CancellationToken ct = default);
        Task<(bool ok, int status, string statusName, string badgeClass)> UpdateStatusAsync(int staffId, int status, CancellationToken ct = default);


    }
}
