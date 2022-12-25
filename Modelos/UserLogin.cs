using System.ComponentModel.DataAnnotations;

namespace MiContabilidad.Modelos
{
    public class UserLogin
    {
        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string UserName { get; set; }


        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 5)]
        public string Password { get; set; }
    }
}
