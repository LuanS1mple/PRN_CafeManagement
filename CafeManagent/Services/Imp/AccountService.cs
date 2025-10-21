using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Mail;
namespace CafeManagent.Services.Imp
{
    public class AccountService : IAccountService
    {
        private readonly IConfiguration _config;

        public AccountService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
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
                string resetLink = $"https://localhost:7088/Account/ResetPassword?email={email}&token={token}";

                var message = new MailMessage(fromEmail, email)
                {
                    Subject = "Khôi phục mật khẩu - Cafe Manager",
                    Body = $"Chào bạn,\n\nNhấn vào liên kết sau để đặt lại mật khẩu:\n{resetLink}\n\nNếu bạn không yêu cầu, hãy bỏ qua email này.",
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

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            // TODO: xử lý cập nhật mật khẩu trong database
            // Ở đây giả lập thành công
            await Task.Delay(300);
            return true;
        }
    }
}
