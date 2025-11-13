using CafeManagent.dto.response;
using CafeManagent.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

// === thêm các using cần thiết ===
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR;
using CafeManagent.Hubs;
using CafeManagent.dto.request.StaffModuleDTO;
using CafeManagent.dto.response.StaffModuleDTO;
using CafeManagent.Services.Interface.StaffModule;

namespace CafeManagent.Services.Imp.StaffModule
{
    public class StaffDirectoryService : IStaffDirectoryService
    {
        private readonly CafeManagementContext _db;
        private readonly IConfiguration _config;
        private readonly IHubContext<StaffHub> _hub;
        private const string DefaultAvatar = "/images/avatars/default.png";
        private const string AvatarFolder = "uploads/avatars"; // giữ đúng yêu cầu

        public StaffDirectoryService(CafeManagementContext db, IConfiguration config, IHubContext<StaffHub> hub)
        {
            _db = db;
            _config = config;
            _hub = hub;
        }
        private static string MapStatusToName(int? s) => s switch
        {
            1 => "Đang làm việc",
            2 => "Nghỉ phép",
            3 => "Nghỉ việc",
            _ => "Không rõ"
        };

        // --- GET PAGE ---
        public async Task<PagedResult<StaffListItemDto>> GetPagedAsync(StaffListQuery q, CancellationToken ct = default)
        {
            var src = _db.Staff.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q.Q))
            {
                var keyword = q.Q.Trim().ToLower();
                src = src.Where(s =>
                    (s.FullName ?? "").ToLower().Contains(keyword) ||
                    (s.Email ?? "").ToLower().Contains(keyword));
            }

            if (q.Status.HasValue)
                src = src.Where(s => s.Status == q.Status.Value);

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
                    s.Status == 1 ? "Đang làm việc" :
                    s.Status == 2 ? "Nghỉ phép" :
                    s.Status == 3 ? "Nghỉ việc" : "Không rõ",
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

        // --- DETAIL ---
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
                    s.UserName != null ? s.UserName : "",
                    s.Address,
                    s.Contract != null ? s.Contract.StartDate : null,
                    s.Contract != null ? s.Contract.EndDate : null,
                    string.IsNullOrWhiteSpace(s.Img) ? DefaultAvatar : s.Img,
                    s.Role != null ? s.Role.RoleName : null,
                    s.Gender,
                    s.RoleId
                ))
                .SingleOrDefaultAsync(ct);
        }

        // --- CREATE ---
        public async Task<int> CreateAsync(CreateStaffRequest req, string webRootPath, CancellationToken ct = default)
        {
            // ====== Validate nghiệp vụ bổ sung ======
            // (Các validate rỗng/định dạng đã do DataAnnotations xử lý ở controller trước khi gọi xuống đây)

            if (req.BaseSalary < 0)
                throw new ValidationException(
                    new ValidationResult("Tiền lương không được âm", new[] { nameof(req.BaseSalary) }), null, req.BaseSalary);

            if (await _db.Staff.AnyAsync(x => x.Email == req.Email, ct))
                throw new ValidationException(
                    new ValidationResult("Email đã tồn tại.", new[] { nameof(req.Email) }), null, req.Email);

            if (await _db.Staff.AnyAsync(x => x.Phone == req.Phone, ct))
                throw new ValidationException(
                    new ValidationResult("Số điện thoại đã tồn tại.", new[] { nameof(req.Phone) }), null, req.Phone);

            if (await _db.Staff.AnyAsync(x => x.UserName == req.UserName, ct))
                throw new ValidationException(
                    new ValidationResult("Tên đăng nhập đã tồn tại.", new[] { nameof(req.UserName) }), null, req.UserName);

            // ====== Random password 6 ký tự (prod: nên hash) ======
            var password = GeneratePassword(6);

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
                Password = password,
                Status = 1,
                CreateAt = DateTime.UtcNow
            };

            _db.Staff.Add(s);
            await _db.SaveChangesAsync(ct); // cần StaffId để đặt tên ảnh

            // --- handle avatar file ---
            if (req.AvatarFile is not null && req.AvatarFile.Length > 0)
            {
                var path = await SaveAvatarFile(req.AvatarFile, webRootPath, s.StaffId, ct);
                s.Img = path;
            }

            // --- tạo hợp đồng 1 năm ---
            var start = DateOnly.FromDateTime(DateTime.Today);
            var end = start.AddYears(1);

            var contract = new Contract
            {
                StaffId = s.StaffId,
                StartDate = start,
                EndDate = end,
                BaseSalary = req.BaseSalary,
                Position = req.Position,
                Status = true,
                SignedDate = DateTime.UtcNow
            };
            _db.Contracts.Add(contract);

            await _db.SaveChangesAsync(ct);

            // --- gửi email mật khẩu (không rollback nếu lỗi gửi) ---
            try
            {
                if (!string.IsNullOrWhiteSpace(s.Email))
                    await SendPasswordEmailAsync(s.Email, s.FullName ?? s.Email, password, ct);
            }
            catch { /* log nếu cần */ }

            return s.StaffId;
        }

        // --- UPDATE ---
        public async Task<bool> UpdateAsync(UpdateStaffProfile dto, IFormFile? avatarFile, string webRootPath, CancellationToken ct = default)
        {
            var s = await _db.Staff
                .Include(x => x.Contract)
                .FirstOrDefaultAsync(x => x.StaffId == dto.StaffId, ct);
            if (s is null) return false;

            // ====== Validate nghiệp vụ (map vào field cụ thể) ======
            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new ValidationException(new ValidationResult("Họ và tên là bắt buộc", new[] { nameof(dto.FullName) }), null, dto.FullName);

            if (string.IsNullOrWhiteSpace(dto.Address))
                throw new ValidationException(new ValidationResult("Địa chỉ là bắt buộc", new[] { nameof(dto.Address) }), null, dto.Address);

            if (string.IsNullOrWhiteSpace(dto.Phone) || !System.Text.RegularExpressions.Regex.IsMatch(dto.Phone, @"^\d{9}$"))
                throw new ValidationException(new ValidationResult("SĐT phải gồm đúng 9 chữ số", new[] { nameof(dto.Phone) }), null, dto.Phone);

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ValidationException(new ValidationResult("Email là bắt buộc", new[] { nameof(dto.Email) }), null, dto.Email);

            // trùng email
            if (await _db.Staff.AnyAsync(x => x.Email == dto.Email && x.StaffId != dto.StaffId, ct))
                throw new ValidationException(new ValidationResult("Email đã tồn tại.", new[] { nameof(dto.Email) }), null, dto.Email);

            // trùng phone
            if (await _db.Staff.AnyAsync(x => x.Phone == dto.Phone && x.StaffId != dto.StaffId, ct))
                throw new ValidationException(new ValidationResult("Số điện thoại đã tồn tại.", new[] { nameof(dto.Phone) }), null, dto.Phone);

            // Contract validations
            if (string.IsNullOrWhiteSpace(dto.Position))
                throw new ValidationException(new ValidationResult("Chức danh (HĐ) là bắt buộc", new[] { nameof(dto.Position) }), null, dto.Position);

            if (!dto.ContractEndDate.HasValue)
                throw new ValidationException(new ValidationResult("Ngày hết hạn HĐ là bắt buộc", new[] { nameof(dto.ContractEndDate) }), null, dto.ContractEndDate);

            // ==== Validate ngày hết hạn mới phải ≥ cũ + 3 tháng (nếu đã có ngày cũ) ====
            if (s.Contract?.EndDate is DateOnly oldEnd)
            {
                var minNew = oldEnd.AddMonths(3);
                if (dto.ContractEndDate.Value < minNew)
                {
                    throw new ValidationException(
                        new ValidationResult(
                            $"Ngày hết hạn HĐ mới phải ≥ {minNew:dd/MM/yyyy} (ít nhất +3 tháng so với {oldEnd:dd/MM/yyyy})",
                            new[] { nameof(dto.ContractEndDate) }
                        ),
                        null,
                        dto.ContractEndDate
                    );
                }
            }
            if (dto.RoleId.HasValue) s.RoleId = dto.RoleId;
            s.FullName = dto.FullName;
            s.Gender = dto.Gender;
            s.BirthDate = dto.BirthDate;
            s.Address = dto.Address;
            s.Phone = dto.Phone;
            s.Email = dto.Email;

            if (s.Contract is null)
            {
                s.Contract = new Contract
                {
                    StaffId = s.StaffId,
                    StartDate = DateOnly.FromDateTime(DateTime.Today),
                    EndDate = dto.ContractEndDate,
                    Position = dto.Position,
                    Status = true,
                    SignedDate = DateTime.UtcNow
                };
                _db.Contracts.Add(s.Contract);
            }
            else
            {
                s.Contract.Position = dto.Position;
                s.Contract.EndDate = dto.ContractEndDate;
            }

            // ====== Avatar ======
            if (avatarFile is not null && avatarFile.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(s.Img) &&
                    s.Img.StartsWith($"/{AvatarFolder}", StringComparison.OrdinalIgnoreCase))
                {
                    var absOld = Path.Combine(webRootPath, s.Img.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    try { if (File.Exists(absOld)) File.Delete(absOld); } catch { }
                }

                var newPath = await SaveAvatarFile(avatarFile, webRootPath, s.StaffId, ct);
                s.Img = newPath;
            }

            await _db.SaveChangesAsync(ct);
            return true;
        }


        // --- SAVE FILE: lưu với tên staff_{staffId}_img.ext ---
        private async Task<string> SaveAvatarFile(IFormFile file, string webRootPath, int staffId, CancellationToken ct)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allow = new[] { ".png", ".jpg", ".jpeg", ".webp" };
            if (!allow.Contains(ext)) throw new InvalidOperationException("Định dạng ảnh không hợp lệ.");
            if (file.Length > 2 * 1024 * 1024) throw new InvalidOperationException("Ảnh vượt quá 2MB.");

            var folder = Path.Combine(webRootPath, AvatarFolder);
            Directory.CreateDirectory(folder);

            var fileName = $"staff_{staffId}_img{ext}";
            var full = Path.Combine(folder, fileName);

            using (var fs = File.Create(full))
                await file.CopyToAsync(fs, ct);

            return $"/{AvatarFolder}/{fileName}".Replace("//", "/");
        }

        // --- gửi mail mật khẩu tạm ---
        private async System.Threading.Tasks.Task SendPasswordEmailAsync(
            string toEmail, string displayName, string password, CancellationToken ct)
        {
            var host = _config["Email:Host"];
            var port = int.Parse(_config["Email:Port"] ?? "587");
            var fromAddress = _config["Email:Address"];
            var appPassword = _config["Email:AppPassword"];

            if (string.IsNullOrWhiteSpace(fromAddress) || string.IsNullOrWhiteSpace(appPassword))
                throw new InvalidOperationException("Thiếu cấu hình Email trong appsettings.json.");

            var from = new MailAddress(fromAddress, "Cafe Management");
            var to = new MailAddress(toEmail, displayName);

            using var msg = new MailMessage(from, to)
            {
                Subject = "Tài khoản nhân viên",
                Body = $"Xin chào {displayName},\n\n" +
                       $"Tài khoản của bạn đã được tạo.\n" +
                       $"Mật khẩu tạm thời: {password}\n\n" +
                       $"Vui lòng đăng nhập và đổi mật khẩu.",
                IsBodyHtml = false
            };

            using var smtp = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(fromAddress, appPassword)
            };

            await smtp.SendMailAsync(msg, ct);
        }


        // mật khẩu ngẫu nhiên 6 ký tự (loại bỏ ký tự dễ nhầm)
        private static string GeneratePassword(int len)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var sb = new StringBuilder(len);
            for (int i = 0; i < len; i++)
            {
                var idx = RandomNumberGenerator.GetInt32(chars.Length);
                sb.Append(chars[idx]);
            }
            return sb.ToString();
        }

        public async Task<List<SelectListItem>> GetRolesSelectListAsync(CancellationToken ct = default)
        {
            return await _db.Roles
                .AsNoTracking()
                .OrderBy(r => r.RoleName)
                .Select(r => new SelectListItem { Value = r.RoleId.ToString(), Text = r.RoleName })
                .ToListAsync(ct);
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        public async Task<(bool ok, int status, string statusName, string badgeClass)> UpdateStatusAsync(int staffId, int status, CancellationToken ct = default)
        {
            var s = await _db.Staff.FirstOrDefaultAsync(x => x.StaffId == staffId, ct);
            if (s is null) return (false, 0, "Không rõ", "badge-gray");

            // ràng buộc 1..3: 1=Đang làm việc, 2=Nghỉ phép, 3=Nghỉ việc
            if (status is < 1 or > 3) status = 0;

            s.Status = status;
            await _db.SaveChangesAsync(ct);

            string name = status switch
            {
                1 => "Đang làm việc",
                2 => "Nghỉ phép",
                3 => "Nghỉ việc",
                _ => "Không rõ"
            };

            // mapping màu theo yêu cầu trong list:
            // xanh nhạt(chấm xanh) = đang làm việc, đỏ = nghỉ việc, xám = nghỉ phép
            string badgeClass = status switch
            {
                1 => "badge-green", // đang làm việc
                3 => "badge-red",   // nghỉ việc
                2 => "badge-gray",  // nghỉ phép
                _ => "badge-gray"
            };

            // 🔔 Phát tín hiệu cho tất cả client đang mở list
            await _hub.Clients.All.SendAsync("ReceiveStatusUpdate", new
            {
                staffId = s.StaffId,
                status,
                name,
                badgeClass
            }, ct);

            return (true, status, name, badgeClass);
        }

    }
}
