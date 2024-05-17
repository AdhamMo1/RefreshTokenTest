using System.ComponentModel.DataAnnotations;

namespace RefreshTokenTest.Dto.Incoming
{
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
