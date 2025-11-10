using CafeManagent.dto.response;
using CafeManagent.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using CafeManagent.dto.request;
using System.Text;

namespace CafeManagent.Services.Imp
{
    public class StaffDirectoryService : IStaffDirectoryService
    {
        private readonly CafeManagementContext _db;
        private const string DefaultAvatar = "/images/avatars/default.png";
        private const string AvatarFolder = "uploads/avatars";

        public StaffDirectoryService(CafeManagementContext db) => _db = db;

        private static string MapStatusToName(int? s) => s switch
        {
            1 => "Đang làm việc",
            2 => "Nghỉ phép",
            3 => "Nghỉ việc",
            _ => "Không rõ"
        };

        // --- GET PAGE / DETAIL (giữ nguyên, chỉ đổi chút nhỏ) ---
        public async Task<PagedResult<StaffListItemDto>> GetPagedAsync(StaffListQuery q, CancellationToken ct = default)
        {
            var src = _db.Staff.AsNoTracking().AsQueryable();

            // Search (SQL Server collation if available)
            if (!string.IsNullOrWhiteSpace(q.Q))
            {
                var keyword = q.Q.Trim().ToLower();
                src = src.Where(s =>
                    (s.FullName ?? "").ToLower().Contains(keyword) ||
                    (s.Email ?? "").ToLower().Contains(keyword));
            }

            if (q.Status.HasValue)
            {
                src = src.Where(s => s.Status == q.Status.Value);
            }

            var total = await src.CountAsync(ct);
            var page = Math.Max(1, q.Page);
            var size = Math.Clamp(q.Size, 5, 100);

            var items = await src
                .OrderBy(s => s.FullName)
                .ThenBy(s => s.StaffId)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(s => new StaffListItemDto(
                    s.StaffId,
                    s.FullName ?? "",
                    s.Email,
                    s.Contract != null ? s.Contract.Position : null,
                    s.Status == 1 ? "Đang làm việc" : s.Status == 2 ? "Nghỉ phép" : s.Status == 3 ? "Nghỉ việc" : "Không rõ",
                    s.Contract != null ? s.Contract.StartDate : null,
                    s.Contract != null ? s.Contract.EndDate : null,
                    string.IsNullOrWhiteSpace(s.Img) ? DefaultAvatar : s.Img
                ))
                .ToListAsync(ct);

            return new PagedResult<StaffListItemDto>
            {
                Items = items,
                Page = page,
                Size = size,
                TotalItems = total
            };
        }

        public async Task<StaffDetailDto?> GetDetailAsync(int id, CancellationToken ct = default)
        {
            return await _db.Staff
                .AsNoTracking()
                .Where(s => s.StaffId == id)
                .Select(s => new StaffDetailDto(
                    s.StaffId,
                    s.FullName,
                    s.Email,
                    s.Contract != null ? s.Contract.Position : null,
                    MapStatusToName(s.Status),
                    s.BirthDate,
                    s.Phone,
                    s.Address,
                    s.Contract != null ? s.Contract.StartDate : null,
                    s.Contract != null ? s.Contract.EndDate : null,
                    string.IsNullOrWhiteSpace(s.Img) ? DefaultAvatar : s.Img,
                    s.Role != null ? s.Role.RoleName : null
                ))
                .SingleOrDefaultAsync(ct);
        }

        // --- NEW: Create staff (accepts IFormFile in controller) ---
        public async Task<int> CreateAsync(CreateStaffRequest req, string webRootPath, CancellationToken ct = default)
        {
            var s = new Staff
            {
                RoleId = req.RoleId,
                FullName = req.FullName,
                Gender = req.Gender,
                BirthDate = req.BirthDate,
                Address = req.Address,
                Phone = req.Phone,
                Email = req.Email,
                UserName = req.UserName,
                Password = req.Password, // production: hash!
                CreateAt = DateTime.UtcNow
            };

            // handle avatar file
            if (req.AvatarFile is not null && req.AvatarFile.Length > 0)
            {
                var path = await SaveAvatarFile(req.AvatarFile, webRootPath, ct);
                s.Img = path;
            }

            _db.Staff.Add(s);
            await _db.SaveChangesAsync(ct);
            return s.StaffId;
        }

        // --- NEW: Update staff (accepts IFormFile) ---
        public async Task<bool> UpdateAsync(UpdateStaffProfile dto, IFormFile? avatarFile, string webRootPath, CancellationToken ct = default)
        {
            var s = await _db.Staff.FirstOrDefaultAsync(x => x.StaffId == dto.StaffId, ct);
            if (s is null) return false;

            s.FullName = dto.FullName;
            s.Gender = dto.Gender;
            s.BirthDate = dto.BirthDate;
            s.Address = dto.Address;
            s.Phone = dto.Phone;
            s.Email = dto.Email;
            // nếu admin muốn đổi role, dto nên có RoleId
            if (dto.RoleId.HasValue) s.RoleId = dto.RoleId;

            // avatar handling
            if (avatarFile is not null && avatarFile.Length > 0)
            {
                // delete old if it's in our uploads folder
                if (!string.IsNullOrWhiteSpace(s.Img) && s.Img.StartsWith($"/{AvatarFolder}"))
                {
                    var absOld = Path.Combine(webRootPath, s.Img.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    try { if (File.Exists(absOld)) File.Delete(absOld); } catch { /* ignore */ }
                }

                var newPath = await SaveAvatarFile(avatarFile, webRootPath, ct);
                s.Img = newPath;
            }

            await _db.SaveChangesAsync(ct);
            return true;
        }

        // --- Helper: save avatar and return public URL e.g. /uploads/avatars/staff_{guid}.jpg
        private async Task<string> SaveAvatarFile(IFormFile file, string webRootPath, CancellationToken ct)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allow = new[] { ".png", ".jpg", ".jpeg", ".webp" };
            if (!allow.Contains(ext)) throw new InvalidOperationException("Định dạng ảnh không hợp lệ.");
            if (file.Length > 2 * 1024 * 1024) throw new InvalidOperationException("Ảnh vượt quá 2MB.");

            var folder = Path.Combine(webRootPath, AvatarFolder);
            Directory.CreateDirectory(folder);

            var fileName = $"staff_{Guid.NewGuid()}{ext}";
            var full = Path.Combine(folder, fileName);

            using (var fs = File.Create(full))
            {
                await file.CopyToAsync(fs, ct);
            }

            // return url path
            return $"/{AvatarFolder}/{fileName}";
        }

        // --- Roles helper for select list in form ---
        public async Task<List<SelectListItem>> GetRolesSelectListAsync(CancellationToken ct = default)
        {
            return await _db.Roles
                .AsNoTracking()
                .OrderBy(r => r.RoleName)
                .Select(r => new SelectListItem { Value = r.RoleId.ToString(), Text = r.RoleName })
                .ToListAsync(ct);
        }

        // Helper bỏ dấu (nếu cần / đang không dùng)
        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var c in normalized)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
