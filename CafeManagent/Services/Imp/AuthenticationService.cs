using CafeManagent.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CafeManagent.Services.Imp
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly string jwt_signer_key;
        private readonly int access_token_duration;
        private readonly int refresh_token_duration;
        private readonly CafeManagementContext _context;
        public AuthenticationService(CafeManagementContext context,IConfiguration configuration)
        {
            jwt_signer_key = configuration["jwt_signer_key"];
            access_token_duration = int.Parse(configuration["access_token_duration"] ?? "3600");
            refresh_token_duration = int.Parse(configuration["refresh_token_duration"] ?? "604800");
            _context = context;
        }
        public string CreateAccessToken(string refreshToken)
        {
           
        }

        public string CreateRefreshToken(string refreshToken)
        {
            string token = null;
            do
            {
                token = Guid.NewGuid().ToString().Substring(0, 8);
            }
            while (_context.RefreshTokens.Where(s => s.Token.Equals(token)).Any());
            RefreshToken newRefreshToken = new RefreshToken
            {
                Token = token,
                ExpireTime = DateTime.UtcNow.AddSeconds(refresh_token_duration),
                IsEnable = true,
                StaffId = GetByRefreshToken(refreshToken).StaffId
            };
            _context.RefreshTokens.Add(newRefreshToken);
            _context.SaveChanges();
            return token;
        }

        private RefreshToken GetByRefreshToken(string oldToken)
        {
            return _context.RefreshTokens.Where(r => r.Token!.Equals(oldToken)).FirstOrDefault()!;
        }

        public ClaimsPrincipal GetClaims(string accessToken)
        {
            JwtSecurityTokenHandler securityTokenHandler = new JwtSecurityTokenHandler();
            TokenValidationParameters parameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_signer_key))
            };
            try
            {
                SecurityToken? token;
                return securityTokenHandler.ValidateToken(accessToken, parameters, out token);
            }
            catch
            {
                return null;
            }
        }

        public bool IsValidAccessToken(string accessToken)
        {
            JwtSecurityTokenHandler securityTokenHandler = new JwtSecurityTokenHandler();
            TokenValidationParameters parameters = new TokenValidationParameters {
                ValidateAudience = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_signer_key))
            };
            try
            {
                SecurityToken? token;
                ClaimsPrincipal claims = securityTokenHandler.ValidateToken(accessToken, parameters,out token);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool IsValidRefreshToken(string refreshToken)
        {
            RefreshToken token = _context.RefreshTokens.Where(r => r.Token.Equals(refreshToken)).FirstOrDefault();
            if(token == null)
            {
                return false;
            }
            if (!token.IsEnable.GetValueOrDefault() || token.ExpireTime == null || token.ExpireTime.Value < DateTime.UtcNow)
            {
                return false;
            }
            return true;
        }

        public string CreateRefreshToken(Staff staff)
        {
            string token = null;
            do
            {
                token = Guid.NewGuid().ToString().Substring(0, 8);
            }
            while (_context.RefreshTokens.Where(s => s.Token.Equals(token)).Any());
            RefreshToken newAccessToken = new RefreshToken
            {
                Token = token,
                ExpireTime = DateTime.UtcNow.AddDays(access_token_duration),
                IsEnable = true,
                StaffId = staff.StaffId
            };
            _context.RefreshTokens.Add(newAccessToken);
            _context.SaveChanges();
            return token;
        }

        public string CreateAccessToken(Staff staff)
        {
           
        }
    }
}
