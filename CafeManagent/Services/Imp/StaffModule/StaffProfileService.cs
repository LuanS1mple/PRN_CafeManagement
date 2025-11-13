using CafeManagent.dto.request.StaffModuleDTO;
using CafeManagent.dto.response.StaffModuleDTO;
using CafeManagent.mapper;
using CafeManagent.Models;
using CafeManagent.Services.Interface.StaffModule;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;

namespace CafeManagent.Services.Imp.StaffModule
{
    public class StaffProfileService : IStaffProfileService
    {
        private readonly CafeManagementContext _db;
        private static readonly string[] _allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };
        private const string AvatarFolder = "uploads/avatars";

        public StaffProfileService(CafeManagementContext db) { _db = db; }

        public async Task<StaffProfile?> GetByIdAsync(int staffId, CancellationToken ct = default)
        {
            return await _db.Staff
                .Where(s => s.StaffId == staffId)
                .Select(s => new StaffProfile(
                    s.StaffId,
                    s.RoleId,
                    s.Role != null ? s.Role.RoleName : null,
                    s.FullName,
                    s.Gender,
                    s.BirthDate,
                    s.Address,
                    s.Phone,
                    s.Email,
                    s.UserName,
                    s.CreateAt,
                    string.IsNullOrWhiteSpace(s.Img) ? "/images/avatars/default.png" : s.Img
                ))
                .AsNoTracking()
                .SingleOrDefaultAsync(ct);
        }

        public async Task<bool> UpdateAsync(UpdateStaffProfile dto, IFormFile? avatarFile, string webRootPath, CancellationToken ct = default)
        {
            var s = await _db.Staff.FirstOrDefaultAsync(x => x.StaffId == dto.StaffId, ct);
            if (s is null) return false;

            // Map DTO -> Entity (không đụng CreateAt, Password...)
            StaffProfileMapper.MapUpdate(dto, s);

            // --- Avatar: đồng bộ với các service khác ---
            if (avatarFile is not null && avatarFile.Length > 0)
            {
                if (avatarFile.Length > 2 * 1024 * 1024)
                    throw new InvalidOperationException("Ảnh vượt quá 2MB.");

                var ext = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
                if (!_allowed.Contains(ext))
                    throw new InvalidOperationException("Định dạng ảnh không hợp lệ.");

                if (!string.IsNullOrWhiteSpace(s.Img) &&
                    s.Img.StartsWith($"/{AvatarFolder}", StringComparison.OrdinalIgnoreCase))
                {
                    var absOld = Path.Combine(
                        webRootPath,
                        s.Img.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                    );

                    try
                    {
                        if (File.Exists(absOld))
                            File.Delete(absOld);
                    }
                    catch
                    {
                    }
                }

                var folder = Path.Combine(webRootPath, AvatarFolder);
                Directory.CreateDirectory(folder);

                var fileName = $"staff_{s.StaffId}_img{ext}";
                var fullPath = Path.Combine(folder, fileName);

                using (var fs = File.Create(fullPath))
                {
                    await avatarFile.CopyToAsync(fs, ct);
                }

                var publicPath = $"/{AvatarFolder}/{fileName}".Replace("//", "/");
                StaffProfileMapper.SetAvatarPath(s, publicPath);
            }

            await _db.SaveChangesAsync(ct);
            return true;
        }

    }
}
