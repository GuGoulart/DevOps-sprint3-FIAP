using Microsoft.EntityFrameworkCore;
using MottuProjeto.Models;

namespace MottuProjeto.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) {}

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Moto> Motos => Set<Moto>();
        public DbSet<Area> Areas => Set<Area>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
