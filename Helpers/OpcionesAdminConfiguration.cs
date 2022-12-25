using MiContabilidad.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiContabilidad.Helpers
{
    public class OpcionesAdminConfiguration : IEntityTypeConfiguration<OpcionesAdministracion>
    {
        public void Configure(EntityTypeBuilder<OpcionesAdministracion> builder)
        {
            builder.HasData(
                new OpcionesAdministracion
                {
                    Id = 1,
                    ParteTaller = 1,
                    PorCada = 5
                }
            );
        }
    }
}
