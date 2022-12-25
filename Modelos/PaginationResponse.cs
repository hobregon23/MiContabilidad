using System.Collections.Generic;

namespace MiContabilidad.Modelos
{
    public class PaginationResponse<T>
    {
        public IEnumerable<T> ListaObjetos { get; set; }

        public int CantPorPag { get; set; }
    }
}
