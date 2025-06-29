using MyApp.Entities;
using MyApp.DataAccess.Repositories;

namespace MyApp.DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IRepository<Usuario> Usuarios { get; }
        public IRepository<Rol> Roles { get; }
        public IRepository<Categoria> Categorias { get; }
        public IRepository<Estado> Estados { get; }
        public IRepository<Articulo> Articulos { get; }
        public IRepository<Prestamo> Prestamos { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Usuarios = new Repository<Usuario>(context);
            Roles = new Repository<Rol>(context);
            Categorias = new Repository<Categoria>(context);
            Estados = new Repository<Estado>(context);
            Articulos = new Repository<Articulo>(context);
            Prestamos = new Repository<Prestamo>(context);
        }

        public async Task<int> SaveAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}

