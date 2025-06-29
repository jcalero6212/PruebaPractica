using MyApp.Entities;
using MyApp.DataAccess.Repositories;

namespace MyApp.DataAccess.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Usuario> Usuarios { get; }
        IRepository<Rol> Roles { get; }
        IRepository<Categoria> Categorias { get; }
        IRepository<Estado> Estados { get; }
        IRepository<Articulo> Articulos { get; }
        IRepository<Prestamo> Prestamos { get; }

        Task<int> SaveAsync();
    }
}

