using RefreshTokenTest.Dto.Outgoing;
using RefreshTokenTest.Models;

namespace RefreshTokenTest.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user);
        RefreshToken CreateRefreshToken();
        void SetRefreshTokenInCookies(RefreshToken refreshToken);
        Task<AuthModel> RefrshTokenAsync(string refreshToken);
    }
}
