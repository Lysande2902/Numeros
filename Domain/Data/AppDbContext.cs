using Microsoft.EntityFrameworkCore;
using ParImparAPI.Domain.Entities;

namespace ParImparAPI.Domain.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Numero> Numeros { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapear la entidad Numero a la tabla base_datos_paridad
            modelBuilder.Entity<Numero>().ToTable("base_datos_paridad");
        }
    }
}
