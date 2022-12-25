using MiContabilidad.Helpers;
using MiContabilidad.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiContabilidad.Data.Servicios
{
    public class CuentasService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly JwtAuthService _jwtAuthService;
        private readonly ILogService _logService;

        public CuentasService(
            UserManager<ApplicationUser> userManager,
            JwtAuthService jwtAuthService,
            ApplicationDbContext context, ILogService logService)
        {
            _jwtAuthService = jwtAuthService;
            _userManager = userManager;
            _context = context;
            _logService = logService;
        }

        public async Task<PaginationResponse<ApplicationUser>> GetPag(Pagination pagination, string name, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin" }))
            {
                return null;
            }

            var queryable = _context.Users.AsQueryable();
            if (!string.IsNullOrEmpty(name) || !string.IsNullOrWhiteSpace(name))
            {
                queryable = queryable.Where(x => x.Name.Contains(name) || x.Email.Contains(name) || x.UserName.Contains(name) || x.LastName.Contains(name));
            }

            double count = await queryable.CountAsync();
            double pagesQuantity = Math.Ceiling(count / pagination.QuantityPerPage);

            return new PaginationResponse<ApplicationUser>() { ListaObjetos = await queryable.Paginate(pagination).ToListAsync(), CantPorPag = (int)pagesQuantity };
        }

        public async Task<string> GetUserRol(string userId)
        {
            try
            {
                var user = await _context.Users.Where(x => x.Id.Equals(userId)).FirstAsync();
                var roleId = (await _context.UserRoles.Where(x => x.UserId.Equals(userId)).FirstAsync()).RoleId;
                return (await _context.Roles.Where(x => x.Id.Equals(roleId)).FirstAsync()).Name;
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task<UserInfo> GetUserInfo(string username, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new UserInfo();
            }

            try
            {
                var user = await _context.Users.Where(x => x.UserName.Equals(username)).FirstAsync();
                if (user == null)
                {
                    return new UserInfo();
                }

                UserInfo userInfo = new UserInfo
                {
                    CreationDate = user.CreationDate,
                    Email = user.Email,
                    LastAccess = user.LastAccess,
                    LastName = user.LastName,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    UserName = user.UserName
                };
                return userInfo;
            }
            catch
            {
                return new UserInfo();
            }
        }

        public async Task<IActionResult> Add(UserInfo model, string rol, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin" }))
            {
                return new ConflictResult();
            }

            if (string.IsNullOrEmpty(rol) || string.IsNullOrWhiteSpace(rol))
                return new ConflictResult();

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Name = model.Name,
                LastName = model.LastName,
                CreationDate = DateTime.Now,
                LastAccessNow = DateTime.Now
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, rol);
                await _logService.Log(_jwtAuthService.GetUsername(token), "Registro Usuario", user.UserName.ToString());
                return new OkResult();
            }
            else
            {
                return new ConflictResult();
            }

        }

        public async Task<IActionResult> ActualizarCuenta(string token, UserUpdate userUpdate)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return null;
            }

            var user = await _context.Users.Where(x => x.UserName.Equals(userUpdate.UserName)).FirstAsync();

            if (string.IsNullOrEmpty(userUpdate.NewPassword) || string.IsNullOrWhiteSpace(userUpdate.NewPassword))
            {
                var resultCheckPass = await _userManager.CheckPasswordAsync(user, userUpdate.OldPassword);
                if (!resultCheckPass)
                {
                    return new ConflictResult();
                }
                else
                {
                    user.Email = userUpdate.Email;
                    user.PhoneNumber = userUpdate.PhoneNumber;
                    _context.Entry(user).State = EntityState.Modified;
                    try
                    {
                        await _context.SaveChangesAsync();
                        await _logService.Log(_jwtAuthService.GetUsername(token), "Actualizado Usuario", user.UserName.ToString());
                    }
                    catch
                    {
                        return new NotFoundResult();
                    }
                    return new OkResult();
                }
            }
            else
            {
                user.Email = userUpdate.Email;
                user.PhoneNumber = userUpdate.PhoneNumber;

                _context.Entry(user).State = EntityState.Modified;

                var result = await _userManager.ChangePasswordAsync(user, userUpdate.OldPassword, userUpdate.NewPassword);
                if (result.Succeeded)
                {
                    try
                    {
                        await _context.SaveChangesAsync();
                        await _logService.Log(_jwtAuthService.GetUsername(token), "Cambio de contraseña de usuario", user.UserName.ToString());
                    }
                    catch
                    {
                        return new NotFoundResult();
                    }
                    return new OkResult();
                }
                else
                {
                    return new ConflictResult();
                }
            }
        }

        public async Task<IActionResult> Eliminar(string userId, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin" }))
            {
                return null;
            }

            try
            {
                var user = await _context.Users.Where(x => x.Id.Equals(userId)).FirstAsync();
                if (user.UserName.Equals("admin"))
                    return new ConflictResult();

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                await _logService.Log(_jwtAuthService.GetUsername(token), "Eliminado Usuario", user.UserName.ToString());
                return new OkResult();
            }
            catch
            {
                return new BadRequestResult();
            }
        }

    }
}
