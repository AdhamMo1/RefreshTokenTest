using Microsoft.AspNetCore.Identity;

namespace RefreshTokenTest.Models
{
    public class ApplicationUser : IdentityUser
    {
        public List<RefreshToken>? RefreshTokens { get; set; }
    }
}
