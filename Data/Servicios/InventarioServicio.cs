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
    public interface IInventarioServicio
    {
        Task<List<Producto>> Get();
        Task<Producto> Get(int id);
        Task<double> GetCantInvertida();
        Task<PaginationResponse<Producto>> GetPag(Pagination pagination, string name, string campoSorteo, string ordenSorteo);
        Task<List<TablaInventario>> GetTop10Cant();
        Task<IActionResult> Add(Producto Producto, string token);
        Task<IActionResult> Update(Producto producto, string token);
        Task<IActionResult> Delete(int id, string token);
        Task<IActionResult> DeleteMultiple(List<Producto> listado, string token);
    }

    public class InventarioServicio : IInventarioServicio
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtAuthService _jwtAuthService;
        private readonly ILogService _logService;

        public InventarioServicio(ApplicationDbContext context,
            JwtAuthService jwtAuthService, ILogService logService)
        {
            _context = context;
            _jwtAuthService = jwtAuthService;
            _logService = logService;
        }

        public async Task<List<Producto>> Get()
        {
            return await _context.Producto.ToListAsync();
        }

        public async Task<Producto> Get(int id)
        {
            var producto = await _context.Producto.FindAsync(id);
            if (producto != null)
                return producto;
            else
                return null;
        }

        public async Task<double> GetCantInvertida()
        {
            var resultado = 0.0;
            var listado = await _context.Producto.ToListAsync();
            foreach (var item in listado)
            {
                resultado += item.Cantidad * item.Costo;
            }
            return resultado;
        }

        public async Task<PaginationResponse<Producto>> GetPag(Pagination pagination, string name, string campoSorteo, string ordenSorteo)
        {
            var queryable = _context.Producto.OrderByDynamic(campoSorteo, ordenSorteo.ToUpper());
            if (!string.IsNullOrEmpty(name) || !string.IsNullOrWhiteSpace(name))
            {
                //var modelo = _context.Modelo.FirstOrDefault(x => x.Nombre.Contains(name));
                //if (modelo != null)
                //{
                //    queryable = queryable.Where(x => x.Descripcion.Contains(name) || x.ModeloId.Equals(modelo.Id));
                //}
                //else
                //{
                //    queryable = queryable.Where(x => x.Descripcion.Contains(name));
                //}
                queryable = queryable.Where(x => x.Descripcion.Contains(name) || x.Modelo.Nombre.Contains(name) || x.Marca.Nombre.Contains(name));
            }
            double count = await queryable.CountAsync();
            double pagesQuantity = Math.Ceiling(count / pagination.QuantityPerPage);

            return new PaginationResponse<Producto>() { ListaObjetos = await queryable.Paginate(pagination).ToListAsync(), CantPorPag = (int)pagesQuantity };
        }

        public async Task<List<TablaInventario>> GetTop10Cant()
        {
            var resultado = await _context.Producto.ToListAsync();
            var categorias = await _context.Categorias.ToListAsync();
            var marcas = await _context.Marca.ToListAsync();
            var tablaInv = new List<TablaInventario>();
            var temp = new List<Producto>();

            foreach (var item in resultado)
            {
                if (temp.Exists(x => x.CategoriaId == item.CategoriaId && x.MarcaId == item.MarcaId))
                {
                    int index = temp.FindIndex(x => x.CategoriaId == item.CategoriaId && x.MarcaId == item.MarcaId);
                    temp[index].Cantidad += item.Cantidad;
                }
                else
                {
                    temp.Add(new Producto() { Cantidad = item.Cantidad, CategoriaId = item.CategoriaId, MarcaId = item.MarcaId });
                }
            }
            foreach (var item in temp)
            {
                var tempI = new TablaInventario()
                {
                    Cantidad = item.Cantidad,
                    NombreCategoria = categorias.Where(x => x.Id.Equals(item.CategoriaId)).ToList()[0].Nombre.Substring(0, 4),
                    NombreMarca = marcas.Where(x => x.Id.Equals(item.MarcaId)).ToList()[0].Nombre
                };
                tablaInv.Add(tempI);
            }
            tablaInv = tablaInv.OrderByDescending(TablaInventario => TablaInventario.Cantidad).Take(10).ToList();

            return tablaInv;
        }

        public async Task<IActionResult> Add(Producto Producto, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }
            //busco si ya existe el producto nuevo
            var productos = await _context.Producto.Include(x=>x.Categoria).Include(x => x.Modelo).Where(
                x => x.ModeloId.Equals(Producto.ModeloId) &&
                x.MarcaId.Equals(Producto.MarcaId) &&
                x.CategoriaId.Equals(Producto.CategoriaId) &&
                x.Costo.Equals(Producto.Costo) &&
                x.Descripcion.Equals(Producto.Descripcion)).ToListAsync();
            
            //si existe actualizo la cantidad
            if (productos.Count != 0)
            {
                productos[0].Cantidad += Producto.Cantidad;
                productos[0].Fecha = Producto.Fecha;
                _context.Entry(productos[0]).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await _logService.Log(_jwtAuthService.GetUsername(token), "Añadir Producto Existente", productos[0].Categoria.Nombre + "-" + productos[0].Modelo.Nombre);
                return new OkResult();
            }

            //si no existe creo uno nuevo
            else
            {
                try
                {
                    _context.Producto.Add(Producto);
                    await _context.SaveChangesAsync();
                    await _logService.Log(_jwtAuthService.GetUsername(token), "Añadir Producto Nuevo", productos[0].Categoria.Nombre + "-" + productos[0].Modelo.Nombre);
                    return new OkResult();
                }
                catch
                {
                    return new ConflictResult();
                }
            }
        }

        public async Task<IActionResult> Update(Producto producto, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }
            if (!ProductoExists(producto.Id))
                return new NotFoundResult();

            _context.Entry(producto).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            var item = await _context.Producto.Include(x => x.Categoria).Include(x => x.Modelo).Where(x => x.Id.Equals(producto.Id)).FirstAsync();
            await _logService.Log(_jwtAuthService.GetUsername(token), "Actualizado Producto", item.Categoria.Nombre + "-" + item.Modelo.Nombre);
            return new OkResult();
        }

        public async Task<IActionResult> Delete(int id, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }
            var producto = await _context.Producto.Include(x => x.Categoria).Include(x => x.Modelo).Where(x=>x.Id.Equals(id)).FirstAsync();
            if (producto == null)
                return new NotFoundResult();

            _context.Producto.Remove(producto);
            await _context.SaveChangesAsync();
            await _logService.Log(_jwtAuthService.GetUsername(token), "Eliminado Producto", producto.Categoria.Nombre + "-" + producto.Modelo.Nombre);
            return new OkResult();
        }

        public async Task<IActionResult> DeleteMultiple(List<Producto> listado, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }

            foreach (var prod in listado)
            {
                if (prod == null)
                    return new NotFoundResult();

                var producto = await _context.Producto.Include(x => x.Categoria).Include(x => x.Modelo).Where(x => x.Id.Equals(prod.Id)).FirstAsync();

                _context.Producto.Remove(prod);
                await _context.SaveChangesAsync();
                await _logService.Log(_jwtAuthService.GetUsername(token), "Eliminado Producto", producto.Categoria.Nombre + "-" + producto.Modelo.Nombre);
            }
            return new OkResult();
        }

        private bool ProductoExists(int id)
        {
            return _context.Producto.Any(e => e.Id == id);
        }
    }
}
