using System;
using System.ComponentModel.DataAnnotations;

namespace MiContabilidad.Modelos
{
    public class ModeloInversion
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "{0} es requerida.")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "{0} es requerido.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "{0} es requerida.")]
        [Range(1, 100000, ErrorMessage = "{0} debe ser mayor que 0.")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "{0} es requerido.")]
        [DataType(DataType.Currency, ErrorMessage = "Cantidad incorrecta")]
        public double Costo { get; set; }
    }
}
