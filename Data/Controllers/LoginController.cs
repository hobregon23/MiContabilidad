using MiContabilidad.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MiContabilidad.Data.Servicios;

namespace MiContabilidad.Data.Controllers
{
    [Route("micontabilidadapi/[controller]")]
    [ApiController]

    public class LoginController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public LoginController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogService logService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _logService = logService;
        }

        [HttpPost]
        public async Task<ActionResult<UserToken>> Login([FromBody] UserLogin userLogin)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(userLogin.UserName, userLogin.Password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    await UpdateLastAccess(userLogin.UserName);
                    await _logService.Log(userLogin.UserName, "Login", "");
                    return await BuildToken(userLogin);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid credentials.");
                    return BadRequest(ModelState);
                }
            }
            // If we got this far, something failed, redisplay form
            return BadRequest(ModelState);
        }

        private async Task<UserToken> BuildToken(UserLogin userLogin)
        {
            var user = _context.Users.Where(x => x.UserName.Equals(userLogin.UserName)).First();

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, userLogin.UserName),
                new Claim(ClaimTypes.Name, userLogin.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.Name),
                new Claim(ClaimTypes.Expiration, String.Format("{0:dd/MM/yyyy hh:mm:ss tt}", DateTime.UtcNow.AddDays(1))),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Tiempo de expiración del token. En nuestro caso lo hacemos de una hora.
            var expiration = DateTime.UtcNow.AddDays(1);

            JwtSecurityToken token = new JwtSecurityToken(
               issuer: null,
               audience: null,
               claims: claims,
               expires: expiration,
               signingCredentials: creds);

            return new UserToken()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }

        public async Task UpdateLastAccess(string username)
        {
            var user = await _context.Users.Where(x => x.UserName.Equals(username)).FirstAsync();

            user.LastAccess = user.LastAccessNow;
            user.LastAccessNow = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                //
            }
        }
    }
}
