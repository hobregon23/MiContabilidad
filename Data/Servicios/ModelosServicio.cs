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
    public interface IModelosServicio
    {
        Task<List<Modelo>> Get();
        Task<ActionResult<Modelo>> Get(int id);
        Task<PaginationResponse<Modelo>> GetPagXMarca(int marcaId, Pagination pagination, string name, string campoSorteo, string ordenSorteo);
        Task<ActionResult<bool>> Exist(string name);
        Task<List<Modelo>> GetModelosUsados();
        Task<List<Modelo>> GetXMarca(int marcaId);
        Task<bool> GetModeloEnUso(int id);
        Task<IActionResult> Add(Modelo modelo, string token);
        Task<IActionResult> Update(Modelo modelo, string token);
        Task<IActionResult> Delete(int id, string token);
        Task<IActionResult> EliminarMultiple(List<Modelo> listado, string token);
    }
    public class ModelosServicio : IModelosServicio
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtAuthService _jwtAuthService;
        private readonly ILogService _logService;

        public ModelosServicio(ApplicationDbContext context,
            JwtAuthService jwtAuthService, ILogService logService)
        {
            _context = context;
            _jwtAuthService = jwtAuthService;
            _logService = logService;
        }

        public async Task<List<Modelo>> Get()
        {
            //_context.Configuration.LazyLoadingEnabled = false;
            return await _context.Modelo.Include(x=>x.Marca).ToListAsync();
        }

        public async Task<ActionResult<Modelo>> Get(int id)
        {
            var modelo = await _context.Modelo.FindAsync(id);
            if (modelo != null)
                return modelo;
            else
                return new NotFoundResult();
        }
        public async Task<PaginationResponse<Modelo>> GetPagXMarca(int marcaId, Pagination pagination, string name, string campoSorteo, string ordenSorteo)
        {
            var queryable = _context.Modelo.Include(x => x.Marca).Where(x => x.MarcaId.Equals(marcaId)).OrderByDynamic(campoSorteo, ordenSorteo.ToUpper());
            if (!string.IsNullOrEmpty(name) || !string.IsNullOrWhiteSpace(name))
            {
                var queryableSearch = _context.Modelo.Include(x => x.Marca).Where(x => x.Nombre.Contains(name)).OrderByDynamic(campoSorteo, ordenSorteo.ToUpper());
                double count = await queryableSearch.CountAsync();
                double pagesQuantity = Math.Ceiling(count / pagination.QuantityPerPage);

                return new PaginationResponse<Modelo>() { ListaObjetos = await queryableSearch.Paginate(pagination).ToListAsync(), CantPorPag = (int)pagesQuantity };
            }
            else
            {
                double count = await queryable.CountAsync();
                double pagesQuantity = Math.Ceiling(count / pagination.QuantityPerPage);

                return new PaginationResponse<Modelo>() { ListaObjetos = await queryable.Paginate(pagination).ToListAsync(), CantPorPag = (int)pagesQuantity };
            }
        }

        public async Task<List<Modelo>> GetXMarca(int marcaId)
        {
            return await _context.Modelo.Include(x => x.Marca).Where(x => x.MarcaId.Equals(marcaId)).ToListAsync();
        }

        public async Task<List<Modelo>> GetModelosUsados()
        {
            List<Modelo> resultado = new List<Modelo>();
            foreach (var item in _context.Modelo)
            {
                var Producto = await _context.Producto.Where(x => x.ModeloId == item.Id).ToListAsync();
                if (Producto.Count != 0)
                    resultado.Add(item);
            }
            return resultado;
        }

        public async Task<bool> GetModeloEnUso(int id)
        {
            var Producto = await _context.Producto.Where(x => x.ModeloId == id).ToListAsync();
            if (Producto.Count == 0)
                return false;
            else
                return true;
        }

        public async Task<IActionResult> Add(Modelo modelo, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }
            try
            {
                _context.Modelo.Add(modelo);
                await _context.SaveChangesAsync();
                await _logService.Log(_jwtAuthService.GetUsername(token), "Añadir Modelo", modelo.Nombre.ToString());
                return new OkResult();
            }
            catch
            {
                return new ConflictResult();
            }
        }

        public async Task<IActionResult> Update(Modelo modelo, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }
            if (!ModeloExists(modelo.Id))
                return new NotFoundResult();

            _context.Entry(modelo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await _logService.Log(_jwtAuthService.GetUsername(token), "Actualizado Modelo", modelo.Nombre.ToString());
            return new OkResult();
        }

        public async Task<IActionResult> Delete(int id, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }

            var usada = await GetModeloEnUso(id);
            if (usada)
            {
                return new ConflictResult();
            }

            var modelo = await _context.Modelo.FindAsync(id);
            if (modelo == null)
                return new NotFoundResult();

            _context.Modelo.Remove(modelo);
            await _context.SaveChangesAsync();
            await _logService.Log(_jwtAuthService.GetUsername(token), "Eliminado Modelo", modelo.Nombre.ToString());
            return new OkResult();
        }

        private bool ModeloExists(int id)
        {
            return _context.Modelo.Any(e => e.Id == id);
        }

        public async Task<ActionResult<bool>> Exist(string name)
        {
            return await _context.Modelo.AnyAsync(e => e.Nombre.Equals(name));
        }

        public async Task<IActionResult> EliminarMultiple(List<Modelo> listado, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }

            foreach (var item in listado)
            {
                if (item == null)
                    return new NotFoundResult();
                var usada = await GetModeloEnUso(item.Id);
                if (usada)
                    return new ConflictResult();

                _context.Modelo.Remove(item);
                await _context.SaveChangesAsync();
                await _logService.Log(_jwtAuthService.GetUsername(token), "Eliminado Modelo", item.Nombre.ToString());
            }
            return new OkResult();
        }
    }
}
