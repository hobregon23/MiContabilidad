using MiContabilidad.Helpers;
using MiContabilidad.Modelos;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MiContabilidad
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                    .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserInRoleConfiguration());
            modelBuilder.ApplyConfiguration(new OpcionesAdminConfiguration());
            modelBuilder.ApplyConfiguration(new TallerConfiguration());
        }

        public DbSet<Taller> Taller { get; set; }

        public DbSet<OpcionesAdministracion> OpcionesAdmin { get; set; }

        public DbSet<Producto> Producto { get; set; }

        public DbSet<Categoria> Categorias { get; set; }

        public DbSet<Marca> Marca { get; set; }

        public DbSet<Modelo> Modelo { get; set; }

        public DbSet<ModeloServicio> Servicio { get; set; }

        public DbSet<ModeloInversion> Inversion { get; set; }

        public DbSet<Log> Logs { get; set; }

    }
}
