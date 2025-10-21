using CafeManagent.dto.request;
using CafeManagent.dto.response;
using CafeManagent.Models;

namespace CafeManagent.Services
{
    public interface IStaffProfileService
    {
        Task<StaffProfile?> GetByIdAsync(int staffId, CancellationToken ct = default);
        Task<bool> UpdateAsync(UpdateStaffProfile dto, IFormFile? avatarFile, string webRootPath, CancellationToken ct = default);
    }
}
