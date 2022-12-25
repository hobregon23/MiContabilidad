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
    public interface IServiciosServicio
    {
        Task<List<ModeloServicio>> GetHoy();
        Task<ActionResult<ModeloServicio>> GetById(int id);
        Task<PaginationResponse<ModeloServicio>> GetPag(Pagination pagination, string name, string campoSorteo, string ordenSorteo);
        Task<List<TablaGanancias>> GetServicioByDate(string periodo);
        Task<IActionResult> Add(ModeloServicio modeloServicio, string token);
        Task<IActionResult> Update(ModeloServicio servicio, string token);
        Task<IActionResult> Delete(int id, string token);
        Task<IActionResult> DeleteMultiple(List<ModeloServicio> listado, string token);
    }

    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class ServiciosServicio : IServiciosServicio
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtAuthService _jwtAuthService;
        private readonly ILogService _logService;

        public ServiciosServicio(ApplicationDbContext context,
            JwtAuthService jwtAuthService, ILogService logService)
        {
            _context = context;
            _jwtAuthService = jwtAuthService;
            _logService = logService;
        }

        public async Task<List<ModeloServicio>> GetHoy()
        {
            return await _context.Servicio.Where(x => x.Fecha.Day.Equals(DateTime.Now.Day) && x.Fecha.Month.Equals(DateTime.Now.Month) && x.Fecha.Year.Equals(DateTime.Now.Year)).ToListAsync();
        }

        public async Task<PaginationResponse<ModeloServicio>> GetPag(Pagination pagination, string name, string campoSorteo, string ordenSorteo)
        {
            var queryable = _context.Servicio.OrderByDynamic(campoSorteo, ordenSorteo.ToUpper());
            if (!string.IsNullOrEmpty(name) || !string.IsNullOrWhiteSpace(name))
            {
                queryable = queryable.Where(x => x.Descripcion.Contains(name) || x.Tipo.Contains(name) || string.Format("{0:dd/MM/yyyy}", x.Fecha).Contains(name));
            }
            double count = await queryable.CountAsync();
            double pagesQuantity = Math.Ceiling(count / pagination.QuantityPerPage);

            return new PaginationResponse<ModeloServicio>() { ListaObjetos = await queryable.Paginate(pagination).ToListAsync(), CantPorPag = (int)pagesQuantity };
        }

        public async Task<ActionResult<ModeloServicio>> GetById(int id)
        {
            var modeloServicio = await _context.Servicio.FindAsync(id);

            if (modeloServicio == null)
            {
                return new NotFoundResult();
            }

            return modeloServicio;
        }

        public async Task<List<TablaGanancias>> GetServicioByDate(string periodo)
        {
            var tabla = new List<TablaGanancias>();
            var servicios = await _context.Servicio.ToListAsync();
            if (periodo.Equals("diario"))
            {
                foreach (var item in servicios)
                {
                    if (item.Fecha.Year == DateTime.Now.Year && item.Fecha.Month == DateTime.Now.Month)
                    {
                        if (tabla.Exists(x => x.Fecha.Day == item.Fecha.Day))
                        {
                            int index = tabla.FindIndex(x => x.Fecha.Day == item.Fecha.Day);
                            tabla[index].Ganancia += item.GananciaNeta;
                        }
                        else
                        {
                            tabla.Add(new TablaGanancias() { Anno = item.Fecha.Year, Fecha = item.Fecha, Ganancia = item.GananciaNeta });
                        }
                    }
                }
            }
            else if (periodo == "general")
            {
                foreach (var item in servicios)
                {
                    tabla.Add(new TablaGanancias() { Anno = item.Fecha.Year, Fecha = item.Fecha, Ganancia = item.GananciaNeta });
                }
            }
            else
            {
                foreach (var item in servicios)
                {
                    if (tabla.Exists(x => x.Anno == item.Fecha.Year && x.Fecha.Month == item.Fecha.Month))
                    {
                        int index = tabla.FindIndex(x => x.Anno == item.Fecha.Year && x.Fecha.Month == item.Fecha.Month);
                        tabla[index].Ganancia += item.GananciaNeta;
                    }
                    else
                    {
                        tabla.Add(new TablaGanancias() { Anno = item.Fecha.Year, Fecha = item.Fecha, Ganancia = item.GananciaNeta });
                    }
                }
            }
            return tabla;
        }

        public async Task<IActionResult> Add(ModeloServicio modeloServicio, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }
            try
            {
                _context.Servicio.Add(modeloServicio);
                await _context.SaveChangesAsync();
                await _logService.Log(_jwtAuthService.GetUsername(token), "Añadir Servicio", modeloServicio.Id.ToString());
                return new OkResult();
            }
            catch
            {
                return new ConflictResult();
            }
        }

        public async Task<IActionResult> Update(ModeloServicio servicio, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }
            if (!ModeloServicioExists(servicio.Id))
                return new NotFoundResult();

            _context.Entry(servicio).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await _logService.Log(_jwtAuthService.GetUsername(token), "Actualizado Servicio", servicio.Id.ToString());
            return new OkResult();
        }

        public async Task<IActionResult> Delete(int id, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }
            var servicio = await _context.Servicio.FindAsync(id);
            if (servicio == null)
                return new NotFoundResult();

            _context.Servicio.Remove(servicio);
            await _context.SaveChangesAsync();
            await _logService.Log(_jwtAuthService.GetUsername(token), "Eliminado Servicio", id.ToString());
            return new OkResult();
        }

        public async Task<IActionResult> DeleteMultiple(List<ModeloServicio> listado, string token)
        {
            if (!await _jwtAuthService.IsAuthorized(token, new List<string>() { "Admin", "User" }))
            {
                return new ConflictResult();
            }

            foreach (var item in listado)
            {
                if (item == null)
                    return new NotFoundResult();

                _context.Servicio.Remove(item);
                await _context.SaveChangesAsync();
                await _logService.Log(_jwtAuthService.GetUsername(token), "Eliminado Servicio", item.Id.ToString());
            }
            return new OkResult();
        }

        private bool ModeloServicioExists(int id)
        {
            return _context.Servicio.Any(e => e.Id == id);
        }
    }
}
