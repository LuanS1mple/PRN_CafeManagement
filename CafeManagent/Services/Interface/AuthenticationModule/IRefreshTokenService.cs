namespace CafeManagent.Services.Interface.AuthenticationModule
{
    public interface IRefreshTokenService
    {
        public Task Clear(DateTime expireTime);
    }
}
