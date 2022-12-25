using MiContabilidad.Helpers;
using MiContabilidad.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiContabilidad.Data.Servicios
{
    public interface IInversionServicio
    {
        Task<List<ModeloInversion>> Get();
        Task<ActionResult<ModeloInversion>> GetById(int id);
        Task<PaginationResponse<ModeloInversion>> GetPag(Pagination pagination, string name, string campoSorteo, string ordenSorteo);
        Task<IActionResult> Add(ModeloInversion modeloInversion, string token);
        Task<IActionResult> Update(ModeloInversion modeloInversion, string token);
        Task<IActionResult> Delete(int id, string token);
        Task<IActionResult> DeleteMultiple(List<ModeloInversion> listado, string token);
    }

    public class InversionServicio : IInversionServicio
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtAuthService _jwtAuthService;
        private readonly ILogService _logService;

        public InversionServicio(ApplicationDbContext context,
            JwtAuthService jwtAuthService, ILogService logService)
        {
            _context = context;
            _jwtAuthService = jwtAuthService;
            _logService = logService;
        }

        public async Task<List<ModeloInversion>> Get()
        {
            return await _context.Inversion.ToListAsync();
        }

        public async Task<ActionResult<ModeloInversion>> GetById(int id)
        {
            var inversion = await _context.Inversion.FindAsync(id);
            if (inversion != null)
                return inversion;
            else
                return new NotFoundResult();
        }

        public async Task<PaginationResponse<ModeloInversion>> GetPag(Pagination pagination, string name, string campoSorteo, string ordenSorteo)
        {
            var queryable = _context.Inversion.OrderByDynamic(campoSorteo, ordenSorteo.ToUpper());
            if (!string.IsNullOrEmpty(name) || !string.IsNullOrWhiteSpace(name))
            {
                queryable = queryable.Where(x => x.Nombre.Contains(name) || string.Format("{0:dd/MM/yyyy}", x.Fecha).Contains(name));
            }
            double count = await queryable.CountAsync();
            double pagesQuantity = Math.Ceiling(count / pagination.QuantityPerPage);

            return new PaginationResponse<ModeloInversion>() { ListaObjetos = await queryable.Paginate(pagination).ToListAsync(), CantPorPag = (int)pagesQuantity };
        }

        public async Task<IActionResult> Add(ModeloInversion modeloInversion, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }
            try
            {
                _context.Inversion.Add(modeloInversion);
                await _context.SaveChangesAsync();
                await _logService.Log(_jwtAuthService.GetUsername(token), "Añadir Inversión", "Costo $" + modeloInversion.Costo + " Detalles " + modeloInversion.Nombre);
                return new OkResult();
            }
            catch
            {
                return new ConflictResult();
            }
        }

        public async Task<IActionResult> Update(ModeloInversion modeloInversion, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }
            if (!ModeloInversionExists(modeloInversion.Id))
                return new NotFoundResult();

            _context.Entry(modeloInversion).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await _logService.Log(_jwtAuthService.GetUsername(token), "Actualizado Inversión", "Costo $" + modeloInversion.Costo + " Detalles " + modeloInversion.Nombre);
            return new OkResult();
        }

        public async Task<IActionResult> Delete(int id, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }
            var modeloInversion = await _context.Inversion.FindAsync(id);
            if (modeloInversion == null)
                return new NotFoundResult();

            _context.Inversion.Remove(modeloInversion);
            await _context.SaveChangesAsync();
            await _logService.Log(_jwtAuthService.GetUsername(token), "Eliminado Inversión", "Costo $" + modeloInversion.Costo + " Detalles " + modeloInversion.Nombre);
            return new OkResult();
        }

        public async Task<IActionResult> DeleteMultiple(List<ModeloInversion> listado, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }

            foreach (var prod in listado)
            {
                if (prod == null)
                    return new NotFoundResult();

                _context.Inversion.Remove(prod);
                await _context.SaveChangesAsync();
                await _logService.Log(_jwtAuthService.GetUsername(token), "Eliminado Inversión", "Costo $" + prod.Costo + " Detalles " + prod.Nombre);
            }
            return new OkResult();
        }

        private bool ModeloInversionExists(int id)
        {
            return _context.Inversion.Any(e => e.Id == id);
        }
    }
}
