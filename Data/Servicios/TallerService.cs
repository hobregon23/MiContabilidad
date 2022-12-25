using MiContabilidad.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiContabilidad.Data.Servicios
{
    public interface ITallerService
    {
        Task<Taller> Get();
        Task<IActionResult> AumentarBalance(double cantidad, string token);
        Task<IActionResult> AumentarBalanceMultiple(List<Producto> listadoInventario, List<ModeloInversion> listadoInversion, string token);
        Task<IActionResult> RestarBalance(double cantidad, string token);
        Task<IActionResult> RestarBalanceMultiple(List<ModeloServicio> listado, string token);
    }

    public class TallerService : ITallerService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtAuthService _jwtAuthService;
        private readonly ILogService _logService;

        public TallerService(ApplicationDbContext context,
            JwtAuthService jwtAuthService, ILogService logService)
        {
            _context = context;
            _jwtAuthService = jwtAuthService;
            _logService = logService;
        }

        public async Task<Taller> Get()
        {
            return await _context.Taller.FirstAsync();
        }

        public async Task<IActionResult> AumentarBalance(double cantidad, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }

            var modelo = await _context.Taller.FirstAsync();
            var old_balance = modelo.Balance;
            modelo.Balance += cantidad;

            _context.Entry(modelo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await _logService.Log(_jwtAuthService.GetUsername(token), "Aumentar Balance", "De $" + old_balance + " A $" + modelo.Balance);
            return new OkResult();
        }

        public async Task<IActionResult> AumentarBalanceMultiple(List<Producto> listadoInventario, List<ModeloInversion> listadoInversion, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }

            var modelo = await _context.Taller.FirstAsync();
            var old_balance = modelo.Balance;

            if (listadoInventario != null)
            {
                foreach (var prod in listadoInventario)
                {
                    modelo.Balance += prod.Cantidad * prod.Costo;
                }
            }

            if (listadoInversion != null)
            {
                foreach (var prod in listadoInversion)
                {
                    modelo.Balance += prod.Costo;
                }
            }

            _context.Entry(modelo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await _logService.Log(_jwtAuthService.GetUsername(token), "Aumentar Balance", "De $" + old_balance + " A $" + modelo.Balance);
            return new OkResult();
        }

        public async Task<IActionResult> RestarBalance(double cantidad, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }

            var modelo = await _context.Taller.FirstAsync();
            var old_balance = modelo.Balance;

            if (modelo.Balance < cantidad)
                return new BadRequestResult();

            modelo.Balance -= cantidad;

            _context.Entry(modelo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await _logService.Log(_jwtAuthService.GetUsername(token), "Restar Balance", "De $" + old_balance + " A $" + modelo.Balance);
            return new OkResult();
        }

        public async Task<IActionResult> RestarBalanceMultiple(List<ModeloServicio> listado, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }

            var modelo = await _context.Taller.FirstAsync();
            var old_balance = modelo.Balance;

            foreach (var prod in listado)
            {
                if (modelo.Balance < prod.ParteTaller)
                    return new BadRequestResult();
                modelo.Balance -= prod.ParteTaller;
            }

            _context.Entry(modelo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await _logService.Log(_jwtAuthService.GetUsername(token), "Restar Balance", "De $" + old_balance + " A $" + modelo.Balance);
            return new OkResult();
        }
    }
}
