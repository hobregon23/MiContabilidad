using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MiContabilidad.Modelos
{
    public class Log
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Action { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
