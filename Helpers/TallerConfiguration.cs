using MiContabilidad.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiContabilidad.Helpers
{
    public class TallerConfiguration : IEntityTypeConfiguration<Taller>
    {
        public void Configure(EntityTypeBuilder<Taller> builder)
        {
            builder.HasData(
                new Taller
                {
                    Id = 1,
                    Balance = 0
                }
            );
        }
    }
}
