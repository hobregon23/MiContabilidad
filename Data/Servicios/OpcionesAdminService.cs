using MiContabilidad.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiContabilidad.Data.Servicios
{
    public interface IOpcionesAdminService
    {
        Task<OpcionesAdministracion> Get();
        Task<IActionResult> Update(OpcionesAdministracion opciones, string token);
    }

    public class OpcionesAdminService : IOpcionesAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtAuthService _jwtAuthService;

        public OpcionesAdminService(
            ApplicationDbContext context,
            JwtAuthService jwtAuthService)
        {
            _context = context;
            _jwtAuthService = jwtAuthService;
        }

        public async Task<OpcionesAdministracion> Get()
        {
            return await _context.OpcionesAdmin.FirstAsync();
        }

        public async Task<IActionResult> Update(OpcionesAdministracion opciones, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin" }))
            {
                return new ConflictResult();
            }

            _context.Entry(opciones).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return new OkResult();
        }
    }
}
