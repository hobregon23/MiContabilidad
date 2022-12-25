using MiContabilidad.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace MiContabilidad.Helpers
{
    public class UserInRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
        {
            builder.HasData(
                new IdentityUserRole<string>
                {
                    RoleId = "77e11a94-548b-4e84-8088-bb9ddd734042",
                    UserId = "a77d4efb-37dd-4e8d-8faf-9f84746fe941"
                }
            );
            
        }
    }
}
