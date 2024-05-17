using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RefreshTokenTest.Dto.Incoming;
using RefreshTokenTest.Dto.Outgoing;
using RefreshTokenTest.Interfaces;
using RefreshTokenTest.Models;

namespace RefreshTokenTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        public AccountsController(Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody]LoginDto loginDto)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email); 
                if (user == null)
                {
                    return NotFound();
                    
                }else
                {
                    var checkPassword = await _userManager.CheckPasswordAsync(user,loginDto.Password);
                    if(checkPassword == false)
                    {
                        return NotFound();
                    }
                    else
                    {
                        if(user.RefreshTokens.Any(x=>x.IsActive))
                        {
                            var ARToken = user.RefreshTokens.FirstOrDefault(x=>x.IsActive);
                            _tokenService.SetRefreshTokenInCookies(ARToken);
                            return Ok(new AuthModel() { Token = _tokenService.CreateToken(user), IsAuthenticated = true, Email = user.Email, UserName = user.UserName, RefreshToken = ARToken.Token, RefreshTokenExpiration =ARToken.ExpiresOn });
                        }
                        var refreshToken = _tokenService.CreateRefreshToken();
                        user.RefreshTokens.Add(refreshToken);
                        await _userManager.UpdateAsync(user);
                        return Ok(new AuthModel() {Token =_tokenService.CreateToken(user),IsAuthenticated = true,Email=user.Email,UserName=user.UserName,RefreshToken = refreshToken.Token,RefreshTokenExpiration=refreshToken.ExpiresOn});
                    }
                }
            }
            return BadRequest();
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Registor([FromBody]RegisterDto registerDto)
        {
            if (ModelState.IsValid)
            {
                
                var user = await _userManager.FindByEmailAsync(registerDto.Email);
                if(user == null)
                {
                    var newUser = new ApplicationUser();
                    newUser.Email = registerDto.Email;
                    newUser.UserName = registerDto.UserName;
                    var isCreated =  await _userManager.CreateAsync(newUser,registerDto.Password);
                    if (isCreated.Succeeded)
                    {
                        var token = new AuthModel() { Token = _tokenService.CreateToken(newUser), IsAuthenticated = true, Email = registerDto.Email, UserName = registerDto.UserName,RefreshToken=_tokenService.CreateRefreshToken().Token,RefreshTokenExpiration=_tokenService.CreateRefreshToken().ExpiresOn };
                       
                        return Ok(token);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return BadRequest();
                }
                
            }
            return BadRequest();
            
        }
    }
}
