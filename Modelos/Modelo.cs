using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiContabilidad.Modelos
{
    public class Modelo
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("MarcaId")]
        public virtual Marca Marca { get; set; }

        [Required(ErrorMessage = "{0} es requerido.")]
        public int MarcaId { get; set; }

        [Required(ErrorMessage = "{0} es requerido.")]
        public string Nombre { get; set; }
    }
}
