using System.ComponentModel.DataAnnotations;

namespace MiContabilidad.Modelos
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "{0} es requerido.")]
        public string Nombre { get; set; }
    }
}
