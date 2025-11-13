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
            var user = await _context.Staff.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) { return false; }
            if (user.Status != 1) { return false; }
            try
            {
                string fromEmail = _config["Email:Address"];
                string appPassword = _config["Email:AppPassword"];

                var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, appPassword),
                    EnableSsl = true
                };

                string token = Guid.NewGuid().ToString();
                _cache.Set($"reset_{email}", token, TimeSpan.FromMinutes(10));
                string resetLink = $"https://localhost:5000/Account/ResetPassword?email={email}&token={token}";

                var message = new MailMessage(fromEmail, email)
                {
                    Subject = "Khôi phục mật khẩu - Cafe Manager",
                    Body = $"Nhấn vào liên kết sau để đặt lại mật khẩu:\n{resetLink}\nLiên kết sẽ hết hạn sau 10 phút.",
                    IsBodyHtml = false
                };

                await smtp.SendMailAsync(message);
                return true;
            }
            catch
            {
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

            user.Password = newPassword;
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


