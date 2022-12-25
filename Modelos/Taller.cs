using System.ComponentModel.DataAnnotations;

namespace MiContabilidad.Modelos
{
    public class Taller
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public double Balance { get; set; }
    }
}
