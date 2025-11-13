using CafeManagent.dto.request.StaffModuleDTO;
using CafeManagent.dto.response.StaffModuleDTO;
using CafeManagent.Models;

namespace CafeManagent.Services.Interface.StaffModule
{
    public interface IStaffProfileService
    {
        Task<StaffProfile?> GetByIdAsync(int staffId, CancellationToken ct = default);
        Task<bool> UpdateAsync(UpdateStaffProfile dto, IFormFile? avatarFile, string webRootPath, CancellationToken ct = default);
    }
}
