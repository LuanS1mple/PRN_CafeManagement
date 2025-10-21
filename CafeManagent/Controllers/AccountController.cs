using CafeManagent.Services;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Vui lòng nhập email.";
                return View();
            }

            bool sent = await _accountService.SendPasswordResetEmailAsync(email);
            if (sent)
            {
                ViewBag.Success = "Đã gửi email đặt lại mật khẩu. Vui lòng kiểm tra hộp thư!";
            }
            else
            {
                ViewBag.Error = "Không thể gửi email. Kiểm tra lại địa chỉ hoặc cấu hình.";
            }
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string token, string newPassword)
        {
            bool ok = await _accountService.ResetPasswordAsync(email, token, newPassword);
            if (ok)
            {
                ViewBag.Success = "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập.";
            }
            else
            {
                ViewBag.Error = "Không thể đặt lại mật khẩu.";
            }
            return View();
        }
    
}
}
