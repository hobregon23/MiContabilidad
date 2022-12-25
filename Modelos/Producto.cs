using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiContabilidad.Modelos
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria Categoria { get; set; }

        [Required(ErrorMessage = "{0} es requerido.")]
        public int CategoriaId { get; set; }

        [ForeignKey("MarcaId")]
        public virtual Marca Marca { get; set; }

        [Required(ErrorMessage = "{0} es requerido.")]
        public int MarcaId { get; set; }

        [ForeignKey("ModeloId")]
        public virtual Modelo Modelo { get; set; }

        [Required(ErrorMessage = "{0} es requerido.")]
        public int ModeloId { get; set; }

        public string Descripcion { get; set; }

        [Required(ErrorMessage = "{0} es requerido.")]
        [Range(1, 10000, ErrorMessage = "{0} debe ser mayor que 0.")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "{0} es requerido.")]
        public double Costo { get; set; }

        [Required(ErrorMessage = "{0} es requerido.")]
        public DateTime Fecha { get; set; }
    }
}
