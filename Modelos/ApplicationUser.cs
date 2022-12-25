using Microsoft.AspNetCore.Identity;
using System;

namespace MiContabilidad.Modelos
{
    public class ApplicationUser : IdentityUser
    {
        [ProtectedPersonalData]
        public string Name { get; set; }
        [ProtectedPersonalData]
        public string LastName { get; set; }
        public DateTime CreationDate { get; set; }
        [ProtectedPersonalData]
        public DateTime LastAccess { get; set; }
        [ProtectedPersonalData]
        public DateTime LastAccessNow { get; set; }
    }
}
