using Microsoft.IdentityModel.Tokens;
using RefreshTokenTest.Interfaces;
using RefreshTokenTest.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Net;
using RefreshTokenTest.Dto.Outgoing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace RefreshTokenTest.Services
{
    public class TokenService : ITokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _Key;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        public TokenService(IConfiguration config, IHttpContextAccessor httpContextAccessor, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
        {
            _config = config;
            _Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Token:Key"]));
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public RefreshToken CreateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var generator = new RNGCryptoServiceProvider();
            generator.GetBytes(randomNumber);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                CreatedOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddDays(10)
            };
        }

        public string CreateToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name,user.UserName)
            };
            var cred = new SigningCredentials(_Key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _config["Token:Issuer"],
                Expires = DateTime.Now.AddMinutes(5),
                SigningCredentials = cred
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<AuthModel> RefrshTokenAsync(string refreshToken)
        {
            var authModel = new AuthModel();
            var user =await _userManager.Users.SingleOrDefaultAsync(x => x.RefreshTokens.Any(x => x.Token == refreshToken));
            if(user == null)
            {
                authModel.IsAuthenticated = false;
                authModel.Message = "Invalid Token";
                return authModel;
               
            }
            var RToken = user.RefreshTokens.SingleOrDefault(x=>x.Token== refreshToken);
            if(!RToken.IsActive)
            {
                authModel.IsAuthenticated= false;
                authModel.Message = "Inactive token";
            }
            RToken.RevokedOn = DateTime.UtcNow;
            var newToken = CreateToken(user);
            var newRefreshToken = CreateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);
            authModel.IsAuthenticated = true;
            authModel.Email = user.Email;
            authModel.RefreshToken = newRefreshToken.Token;
            authModel.Token = newToken;
            authModel.UserName = user.UserName;
            return authModel;
        }

        public void SetRefreshTokenInCookies(RefreshToken refreshToken)
        {
            var cookies = new CookieOptions()
            {
                HttpOnly = true,
                Expires = refreshToken.ExpiresOn.ToLocalTime()
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("RefreshToken", refreshToken.Token);
        }
    }
}
