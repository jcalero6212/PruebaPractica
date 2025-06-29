using MyApp.Entities;

namespace MyApp.Business.Services
{
    public interface IUsuarioService
    {
        Task<Usuario?> RegistrarUsuarioAsync(string nombre, string email, string password);
        Task<Usuario?> AutenticarAsync(string email, string password);
        Task CambiarRolAsync(int usuarioId, int rolId);
    }
}

