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
    public interface ICategoriasServicio
    {
        Task<List<Categoria>> Get();
        Task<ActionResult<Categoria>> Get(int id);
        Task<PaginationResponse<Categoria>> GetPag(Pagination pagination, string name, string campoSorteo, string ordenSorteo);
        Task<List<Categoria>> GetCatUsadas();
        Task<bool> CatEnUso(int id);
        Task<IActionResult> Add(Categoria categoria, string token);
        Task<IActionResult> Update(Categoria categoria, string token);
        Task<IActionResult> Delete(int id, string token);
        Task<IActionResult> EliminarMultiple(List<Categoria> listado, string token);
    }
    public class CategoriasServicio : ICategoriasServicio
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtAuthService _jwtAuthService;
        private readonly ILogService _logService;

        public CategoriasServicio(ApplicationDbContext context,
            JwtAuthService jwtAuthService, ILogService logService)
        {
            _context = context;
            _jwtAuthService = jwtAuthService;
            _logService = logService;
        }

        public async Task<List<Categoria>> Get()
        {
            return await _context.Categorias.ToListAsync();
        }

        public async Task<ActionResult<Categoria>> Get(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria != null)
                return categoria;
            else
                return new NotFoundResult();
        }

        public async Task<PaginationResponse<Categoria>> GetPag(Pagination pagination, string name, string campoSorteo, string ordenSorteo)
        {
            var queryable = _context.Categorias.OrderByDynamic(campoSorteo, ordenSorteo.ToUpper());
            if (!string.IsNullOrEmpty(name) || !string.IsNullOrWhiteSpace(name))
            {
                queryable = queryable.Where(x => x.Nombre.Contains(name)).OrderByDynamic(campoSorteo, ordenSorteo.ToUpper());
            }
            double count = await queryable.CountAsync();
            double pagesQuantity = Math.Ceiling(count / pagination.QuantityPerPage);

            return new PaginationResponse<Categoria>() { ListaObjetos = await queryable.Paginate(pagination).ToListAsync(), CantPorPag = (int)pagesQuantity };
        }

        public async Task<List<Categoria>> GetCatUsadas()
        {
            List<Categoria> resultado = new List<Categoria>();
            foreach (var item in _context.Categorias)
            {
                var Producto = await _context.Producto.Where(x => x.CategoriaId == item.Id).ToListAsync();
                if (Producto.Count != 0)
                    resultado.Add(item);
            }
            return resultado;
        }

        public async Task<bool> CatEnUso(int id)
        {
            var Producto = await _context.Producto.Where(x => x.CategoriaId == id).ToListAsync();
            if (Producto.Count == 0)
                return false;
            else
                return true;
        }

        public async Task<IActionResult> Add(Categoria categoria, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }
            try
            {
                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();
                await _logService.Log(_jwtAuthService.GetUsername(token), "Añadir Categoría", categoria.Nombre.ToString());
                return new OkResult();
            }
            catch
            {
                return new ConflictResult();
            }
        }

        public async Task<IActionResult> Update(Categoria categoria, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }
            if (!CategoriaExists(categoria.Id))
                return new NotFoundResult();

            _context.Entry(categoria).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await _logService.Log(_jwtAuthService.GetUsername(token), "Actualizado Categoría", categoria.Nombre.ToString());
            return new OkResult();
        }

        public async Task<IActionResult> Delete(int id, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }
            var cat = await _context.Categorias.FindAsync(id);
            if (cat == null)
                return new NotFoundResult();
            var usada = await CatEnUso(id);
            if (usada)
                return new ConflictResult();

            _context.Categorias.Remove(cat);
            await _context.SaveChangesAsync();
            await _logService.Log(_jwtAuthService.GetUsername(token), "Eliminado Categoría", cat.Nombre.ToString());
            return new OkResult();
        }

        public async Task<IActionResult> EliminarMultiple(List<Categoria> listado, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }

            foreach (var item in listado)
            {
                if (item == null)
                    return new NotFoundResult();
                var usada = await CatEnUso(item.Id);
                if (usada)
                    return new ConflictResult();

                _context.Categorias.Remove(item);
                await _context.SaveChangesAsync();
                await _logService.Log(_jwtAuthService.GetUsername(token), "Eliminado Categoría", item.Nombre.ToString());
            }
            return new OkResult();
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.Id == id);
        }
    }
}
