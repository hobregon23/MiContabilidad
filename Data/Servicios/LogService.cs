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
    public interface ILogService
    {
        Task Log(string userName, string action, string description);
        Task<PaginationResponse<Log>> GetPag(Pagination pagination, string name, string campoSorteo, string ordenSorteo);
    }

    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;

        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginationResponse<Log>> GetPag(Pagination pagination, string name, string campoSorteo, string ordenSorteo)
        {
            var queryable = _context.Logs.OrderByDynamic(campoSorteo, ordenSorteo.ToUpper());
            if (!string.IsNullOrEmpty(name) || !string.IsNullOrWhiteSpace(name))
            {
                queryable = queryable.Where(x => x.Description.Contains(name)).OrderByDynamic(campoSorteo, ordenSorteo.ToUpper());
            }
            double count = await queryable.CountAsync();
            double pagesQuantity = Math.Ceiling(count / pagination.QuantityPerPage);

            return new PaginationResponse<Log>() { ListaObjetos = await queryable.Paginate(pagination).ToListAsync(), CantPorPag = (int)pagesQuantity };
        }

        public async Task Log(string userName, string action, string description)
        {
            var item = new Log()
            {
                Fecha = DateTime.Now,
                UserName = userName,
                Action = action,
                Description = description
            };
            try
            {
                _context.Logs.Add(item);
                await _context.SaveChangesAsync();
                return;
            }
            catch
            {
                return;
            }
        }
    }
}
