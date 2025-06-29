using MyApp.DataAccess.UnitOfWork;
using MyApp.Entities;
using Microsoft.AspNetCore.Identity;

namespace MyApp.Business.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public UsuarioService(IUnitOfWork unitOfWork, IPasswordHasher<Usuario> passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        public async Task<Usuario?> RegistrarUsuarioAsync(string nombre, string email, string password)
        {
            if (await _unitOfWork.Usuarios.ExistsAsync(u => u.Email == email))
                return null;

            var usuario = new Usuario
            {
                Nombre = nombre,
                Email = email,
                RolId = 2 // Operador por defecto
            };

            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, password);

            await _unitOfWork.Usuarios.AddAsync(usuario);
            await _unitOfWork.SaveAsync();

            return usuario;
        }


        public async Task<Usuario?> AutenticarAsync(string email, string password)
        {
            var usuario = (await _unitOfWork.Usuarios.GetAllAsync())
                .FirstOrDefault(u => u.Email == email);

            if (usuario == null) return null;

            var result = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);
            return result == PasswordVerificationResult.Success ? usuario : null;
        }

        public async Task CambiarRolAsync(int usuarioId, int rolId)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId);
            if (usuario == null) throw new Exception("Usuario no encontrado");

            usuario.RolId = rolId;
            _unitOfWork.Usuarios.Update(usuario);
            await _unitOfWork.SaveAsync();
        }
    }
}

