using Microsoft.EntityFrameworkCore;
using MyApp.Entities;

namespace MyApp.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Rol> Roles => Set<Rol>();
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Estado> Estados => Set<Estado>();
        public DbSet<Articulo> Articulos => Set<Articulo>();
        public DbSet<Prestamo> Prestamos => Set<Prestamo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Articulo>()
                .HasIndex(a => a.Codigo)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Rol>().HasData(
                new Rol { Id = 1, Nombre = "Administrador" },
                new Rol { Id = 2, Nombre = "Operador" }
            );

            modelBuilder.Entity<Estado>().HasData(
                new Estado { Id = 1, Nombre = "Disponible" },
                new Estado { Id = 2, Nombre = "Prestado" },
                new Estado { Id = 3, Nombre = "Dañado" }
            );
        }
    }
}


