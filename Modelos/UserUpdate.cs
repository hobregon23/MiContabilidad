using System.ComponentModel.DataAnnotations;

namespace MiContabilidad.Modelos
{
    public class UserUpdate
    {
        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 5)]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 5)]
        public string NewPassword { get; set; }

        [Compare("NewPassword")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; }
    }
}
