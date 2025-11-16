using CafeManagent.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using CafeManagent.Services.Interface.StaffModule;

namespace CafeManagent.Services.Imp.StaffModule
{
    public class AccountService : IAccountService
    {
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly CafeManagementContext _context;

        public AccountService(IConfiguration config, IMemoryCache cache, CafeManagementContext context)
        {
            _config = config;
            _cache = cache;
            _context = context;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            // Cần đảm bảo rằng _config (IConfiguration) và _context (DbContext) đã được inject
            // và _cache (IMemoryCache) đã được inject

            // 1. Kiểm tra User và Trạng thái
            var user = await _context.Staff.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) { return false; }
            if (user.Status != 1) { return false; } // Chỉ cho phép đặt lại mật khẩu nếu Status = 1 (Active)

            try
            {
                // 2. Lấy cấu hình Email từ appsettings.json (Áp dụng từ hàm mẫu)
                var host = _config["Email:Host"];
                var port = int.Parse(_config["Email:Port"] ?? "587");
                var fromAddress = _config["Email:Address"];
                var appPassword = _config["Email:AppPassword"];

                if (string.IsNullOrWhiteSpace(fromAddress) || string.IsNullOrWhiteSpace(appPassword))
                    throw new InvalidOperationException("Thiếu cấu hình Email (Host, Port, Address, AppPassword) trong appsettings.json.");

                // 3. Tạo Token và Link Reset
                string token = Guid.NewGuid().ToString();
                // Giả sử _cache là IMemoryCache
                _cache.Set($"reset_{email}", token, TimeSpan.FromMinutes(10));
                // Đảm bảo host này là host chạy ứng dụng của bạn
                string resetLink = $"http://localhost:5211/Account/ResetPassword?email={email}&token={token}";

                // 4. Cấu hình Email (Sử dụng MailAddress và MailMessage)
                var from = new MailAddress(fromAddress, "Cafe Management");
                var to = new MailAddress(email, user.FullName); // Sử dụng FullName làm displayName

                using var message = new MailMessage(from, to)
                {
                    Subject = "Khôi phục mật khẩu - Cafe Manager",
                    Body = $"Xin chào {user.FullName},\n\n" +
                           $"Chúng tôi nhận được yêu cầu đặt lại mật khẩu tài khoản của bạn.\n\n" +
                           $"Vui lòng nhấn vào liên kết sau để đặt lại mật khẩu:\n{resetLink}\n\n" +
                           $"Liên kết này sẽ hết hạn sau 10 phút. Nếu bạn không yêu cầu thay đổi mật khẩu, vui lòng bỏ qua email này.",
                    IsBodyHtml = false
                };

                // 5. Cấu hình SmtpClient (Sử dụng cấu hình từ appsettings.json)
                using var smtp = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(fromAddress, appPassword)
                };
                await smtp.SendMailAsync(message, CancellationToken.None);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi email khôi phục mật khẩu cho {email}: {ex.Message}");
                return false;
            }
        }

        public async Task<string> ResetPasswordAsync(string email, string token, string newPassword)
        {
            if (!_cache.TryGetValue($"reset_{email}", out string? savedToken))
                return "InvalidToken";

            if (savedToken != token)
                return "InvalidToken";

            var user = await _context.Staff.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return "UserNotFound";

            if (user.Password == newPassword)
                return "SameAsOld";

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

            user.Password = hashedPassword;
            await _context.SaveChangesAsync();


            _cache.Remove(token);
            _cache.Remove($"reset_{email}");

            return "Success";
        }


        public bool IsValidResetToken(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return false;

            return _cache.TryGetValue($"reset_{email}", out string? savedToken) && savedToken == token;
        }

    }

}
