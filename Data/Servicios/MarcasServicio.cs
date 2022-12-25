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
    public interface IMarcasServicio
    {
        Task<List<Marca>> Get();
        Task<ActionResult<Marca>> Get(int id);
        Task<PaginationResponse<Marca>> GetPag(Pagination pagination, string name, string campoSorteo, string ordenSorteo);
        Task<List<Marca>> GetMarcasUsadas();
        Task<bool> GetMarcaEnUso(int id);
        Task<IActionResult> Add(Marca marca, string token);
        Task<IActionResult> Update(Marca marca, string token);
        Task<IActionResult> Delete(int id, string token);
        Task<IActionResult> EliminarMultiple(List<Marca> listado, string token);
    }
    public class MarcasServicio : IMarcasServicio
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtAuthService _jwtAuthService;
        private readonly ILogService _logService;

        public MarcasServicio(ApplicationDbContext context,
            JwtAuthService jwtAuthService, ILogService logService)
        {
            _context = context;
            _jwtAuthService = jwtAuthService;
            _logService = logService;
        }

        public async Task<List<Marca>> Get()
        {
            return await _context.Marca.ToListAsync();
        }

        public async Task<ActionResult<Marca>> Get(int id)
        {
            var marca = await _context.Marca.FindAsync(id);
            if (marca != null)
                return marca;
            else
                return new NotFoundResult();
        }

        public async Task<PaginationResponse<Marca>> GetPag(Pagination pagination, string name, string campoSorteo, string ordenSorteo)
        {
            var queryable = _context.Marca.OrderByDynamic(campoSorteo, ordenSorteo.ToUpper());
            if (!string.IsNullOrEmpty(name) || !string.IsNullOrWhiteSpace(name))
            {
                queryable = queryable.Where(x => x.Nombre.Contains(name)).OrderByDynamic(campoSorteo, ordenSorteo.ToUpper());
            }            
            double count = await queryable.CountAsync();
            double pagesQuantity = Math.Ceiling(count / pagination.QuantityPerPage);
            
            return new PaginationResponse<Marca>() { ListaObjetos = await queryable.Paginate(pagination).ToListAsync(), CantPorPag = (int)pagesQuantity };
        }

        public async Task<List<Marca>> GetMarcasUsadas()
        {
            List<Marca> resultado = new List<Marca>();
            foreach (var item in _context.Marca)
            {
                var Producto = await _context.Producto.Where(x => x.MarcaId == item.Id).ToListAsync();
                if (Producto.Count != 0)
                    resultado.Add(item);
            }
            return resultado;
        }

        public async Task<bool> GetMarcaEnUso(int id)
        {
            var Producto = await _context.Producto.Where(x => x.MarcaId == id).ToListAsync();
            var Modelo = await _context.Modelo.Where(x => x.MarcaId == id).ToListAsync();
            if (Producto.Count == 0 && Modelo.Count == 0)
                return false;
            else
                return true;
        }

        public async Task<IActionResult> Add(Marca marca, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }
            try
            {
                _context.Marca.Add(marca);
                await _context.SaveChangesAsync();
                await _logService.Log(_jwtAuthService.GetUsername(token), "Añadir Marca", marca.Nombre.ToString());
                return new OkResult();
            }
            catch
            {
                return new ConflictResult();
            }
        }

        public async Task<IActionResult> Update(Marca marca, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }
            if (!MarcaExists(marca.Id))
                return new NotFoundResult();

            _context.Entry(marca).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await _logService.Log(_jwtAuthService.GetUsername(token), "Actualizado Marca", marca.Nombre.ToString());
            return new OkResult();
        }

        public async Task<IActionResult> Delete(int id, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }
            var marca = await _context.Marca.FindAsync(id);
            if (marca == null)
                return new NotFoundResult();
            var usada = await GetMarcaEnUso(id);
            if (usada)
                return new ConflictResult();

            _context.Marca.Remove(marca);
            await _context.SaveChangesAsync();
            await _logService.Log(_jwtAuthService.GetUsername(token), "Eliminado Marca", marca.Nombre.ToString());
            return new OkResult();
        }

        public async Task<IActionResult> EliminarMultiple(List<Marca> listado, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }

            foreach (var item in listado)
            {
                if (item == null)
                    return new NotFoundResult();
                var usada = await GetMarcaEnUso(item.Id);
                if (usada)
                    return new ConflictResult();

                _context.Marca.Remove(item);
                await _context.SaveChangesAsync();
                await _logService.Log(_jwtAuthService.GetUsername(token), "Eliminado Marca", item.Nombre.ToString());
            }
            return new OkResult();
        }

        private bool MarcaExists(int id)
        {
            return _context.Marca.Any(e => e.Id == id);
        }
    }
}
