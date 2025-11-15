using CafeManagent.dto.request.StaffModuleDTO;
using CafeManagent.dto.response.StaffModuleDTO;
using CafeManagent.mapper;
using CafeManagent.Models;
using CafeManagent.Services.Interface.StaffModule;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

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

            // Email trùng với người khác
            if (await _db.Staff.AnyAsync(x => x.Email == dto.Email && x.StaffId != dto.StaffId, ct))
                throw new ValidationException(
                    new ValidationResult("Email đã tồn tại.", new[] { nameof(dto.Email) }),
                    null,
                    dto.Email
                );

            // Phone trùng với người khác
            if (await _db.Staff.AnyAsync(x => x.Phone == dto.Phone && x.StaffId != dto.StaffId, ct))
                throw new ValidationException(
                    new ValidationResult("Số điện thoại đã tồn tại.", new[] { nameof(dto.Phone) }),
                    null,
                    dto.Phone
                );

            // Ngày sinh: không ở tương lai, không trước 01/01/1900
            if (dto.BirthDate.HasValue)
            {
                var d = dto.BirthDate.Value;
                var min = new DateOnly(1900, 1, 1);
                var today = DateOnly.FromDateTime(DateTime.Today);

                if (d < min)
                {
                    throw new ValidationException(
                        new ValidationResult("Ngày sinh phải từ 01/01/1900 trở đi", new[] { nameof(dto.BirthDate) }),
                        null,
                        dto.BirthDate
                    );
                }

                if (d > today)
                {
                    throw new ValidationException(
                        new ValidationResult("Ngày sinh không được là ngày trong tương lai", new[] { nameof(dto.BirthDate) }),
                        null,
                        dto.BirthDate
                    );
                }
            }

            StaffProfileMapper.MapUpdate(dto, s);

            // ====== AVATAR ======
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
                    catch { }
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

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest dto, CancellationToken ct = default)
        {
            var user = await _db.Staff.FirstOrDefaultAsync(x => x.StaffId == dto.StaffId, ct);
            if (user is null) return false;

            // Hash mật khẩu mới với BCrypt
            user.Password = HashPassword(dto.NewPassword);

            await _db.SaveChangesAsync(ct);
            return true;
        }

        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }


    }
}
