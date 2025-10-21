namespace CafeManagent.Services
{
    public interface IAccountService
    {
        Task<bool> SendPasswordResetEmailAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    }
}
