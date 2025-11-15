using CafeManagent.Services;
using CafeManagent.Services.Interface.StaffModule;
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewData["Error"] = "Vui lòng nhập email.";
                return View(); // trả về trực tiếp View, không redirect
            }

            bool sent = await _accountService.SendPasswordResetEmailAsync(email);

            if (sent)
            {
                ViewData["Success"] = " Đã gửi email xác nhận khôi phục mật khẩu! Vui lòng kiểm tra hộp thư của bạn.";
            }
            else
            {
                ViewData["Error"] = " Email không tồn tại hoặc tài khoản đã bị khóa.";
            }

            return View(); // trả về View trực tiếp
        }


        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Home");
            }

            if (!_accountService.IsValidResetToken(email, token))
            {
                return RedirectToAction("Login", "Home");
            }

            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email, string token, string newPassword)
        {
            string result = await _accountService.ResetPasswordAsync(email, token, newPassword);

            switch (result)
            {
                case "Success":
                    TempData["SuccessReset"] = "✅ Đặt lại mật khẩu thành công!";
                    return RedirectToAction("ResetPassword", new { email, token });

                case "SameAsOld":
                    ModelState.AddModelError("", "Mật khẩu mới không được trùng mật khẩu cũ.");
                    break;

                case "InvalidToken":
                    TempData["ErrorReset"] = "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                    return RedirectToAction("ForgotPassword");

                case "UserNotFound":
                    TempData["ErrorReset"] = "Không tìm thấy tài khoản tương ứng với email này.";
                    return RedirectToAction("ForgotPassword");

                default:
                    ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình đặt lại mật khẩu. Vui lòng thử lại.");
                    break;
            }
            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

    }
}


