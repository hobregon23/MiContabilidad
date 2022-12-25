using MiContabilidad.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiContabilidad.Data.Servicios
{
    public class JwtAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public JwtAuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _configuration = configuration;
            _userManager = userManager;
            _context = context;
        }

        public string GetUsername(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["jwt:key"]))
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var username = jwtToken.Claims.First(x => x.Type == "unique_name").Value;

            return username;
        }

        public async Task<bool> IsAuthorized(string token, List<string> roles)
        {
            //var token = await JS.GetFromLocalStorage("TOKENKEY");
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_configuration["jwt:key"]))
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                if (DateTime.UtcNow.CompareTo(Convert.ToDateTime(jwtToken.Claims.First(x => x.Type.Contains("piration")).Value)) > 0)
                    return false;
                var username = jwtToken.Claims.First(x => x.Type == "unique_name").Value;
                if (username == null)
                    return false;

                var user = await _context.Users.Where(x => x.UserName.Equals(username)).FirstAsync();
                foreach (var rol in roles)
                {
                    if (await _userManager.IsInRoleAsync(user, rol))
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
