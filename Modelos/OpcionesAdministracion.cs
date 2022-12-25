using System.ComponentModel.DataAnnotations;

namespace MiContabilidad.Modelos
{
    public class OpcionesAdministracion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ParteTaller { get; set; }

        [Required]
        public int PorCada { get; set; }
    }
}
