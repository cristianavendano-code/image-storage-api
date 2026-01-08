using image_storage_API.Models;
using image_storage_API.Models.DTOs;

namespace image_storage_API.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Autentica un usuario y genera JWT
        /// Devuelve token si las credenciales son correctas, null si no
        /// </summary>
        string? Login(LoginDTO dto);

        /// <summary>
        /// Registra un nuevo usuario
        /// Devuelve el usuario creado si el username no existe, null si ya existe
        /// </summary>
        UserModel? Register(RegisterDTO dto);

        /// <summary>
        /// Verifica si un username ya existe
        /// </summary>
        bool UserExists(string username);
    }
}
