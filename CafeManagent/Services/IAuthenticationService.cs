using CafeManagent.Models;
using System.Security.Claims;

namespace CafeManagent.Services
{
    public interface IAuthenticationService
    {
        string CreateAccessToken(string refreshToken);
        string CreateAccessToken(Staff staff);
        Task<string> CreateRefreshToken(string refreshToken);
        Task<string> CreateRefreshToken(Staff staff);
        void DisableRefreshToken(string refreshToken);
        void DisableRefreshToken(int staffId);
        ClaimsPrincipal GetClaims(string accessToken);
        public bool IsValidAccessToken(string refreshToken);
        public bool IsValidRefreshToken(string accessToken);
    }
}
