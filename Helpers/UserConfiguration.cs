using MiContabilidad.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace MiContabilidad.Helpers
{
    public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.HasData(
                new ApplicationUser
                {
                    Id = "a77d4efb-37dd-4e8d-8faf-9f84746fe941",
                    CreationDate = DateTime.Now,
                    Name = "Administrador",
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    Email = "admin@nauta.cu",
                    NormalizedEmail = "ADMIN@NAUTA.CU",
                    PhoneNumber = "53604366",
                    LastName = "del Sistema",
                    PasswordHash = "AQAAAAEAACcQAAAAEDsAtWGJgjqyvMqMcQVOGaZ0HReabnNrmol+mpQK+2gZJzyVr+s0A7xbT8OIYs9yyw=="
                }
            );
        }
    }
}
