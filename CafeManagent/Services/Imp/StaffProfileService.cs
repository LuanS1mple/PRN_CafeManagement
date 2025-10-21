using CafeManagent.dto.request;
using CafeManagent.dto.response;
using CafeManagent.mapper;
using CafeManagent.Models;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Services.Imp
{
    public class StaffProfileService : IStaffProfileService
    {
        private readonly CafeManagementContext _db;
        private static readonly string[] _allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };

        public StaffProfileService(CafeManagementContext db) { _db = db; }

        public async Task<StaffProfile?> GetByIdAsync(int staffId, CancellationToken ct = default)
        {
            return await _db.Staff
                .Where(s => s.StaffId == staffId)
                .Select(s => new StaffProfile(
                    s.StaffId,
                    s.RoleId,
                    s.Role != null ? s.Role.RoleName : null,   // <-- map trực tiếp
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
                .AsNoTracking()               // đọc thì nên NoTracking cho nhẹ và an toàn
                .SingleOrDefaultAsync(ct);
        }


        public async Task<bool> UpdateAsync(UpdateStaffProfile dto, IFormFile? avatarFile, string webRootPath, CancellationToken ct = default)
        {
            var s = await _db.Staff.FirstOrDefaultAsync(x => x.StaffId == dto.StaffId, ct);
            if (s is null) return false;

            // Map DTO -> Entity
            StaffProfileMapper.MapUpdate(dto, s);

            // Upload avatar (nếu có)
            if (avatarFile is not null && avatarFile.Length > 0)
            {
                if (avatarFile.Length > 2 * 1024 * 1024)
                    throw new InvalidOperationException("Ảnh vượt quá 2MB.");

                var ext = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
                if (!_allowed.Contains(ext))
                    throw new InvalidOperationException("Định dạng ảnh không hợp lệ.");

                var folder = Path.Combine(webRootPath, "uploads", "avatars");
                Directory.CreateDirectory(folder);
                var fileName = $"staff_{s.StaffId}{ext}";
                var fullPath = Path.Combine(folder, fileName);
                using var fs = File.Create(fullPath);
                await avatarFile.CopyToAsync(fs, ct);

                StaffProfileMapper.SetAvatarPath(s, $"/uploads/avatars/{fileName}");
            }

            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
