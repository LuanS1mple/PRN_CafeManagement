namespace CafeManagent.Services
{
    public interface IAccountService
    {
        Task<bool> SendPasswordResetEmailAsync(string email);
        Task<string> ResetPasswordAsync(string email, string token, string newPassword);
        bool IsValidResetToken(string email, string token);
    }
}
