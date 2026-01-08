using Microsoft.AspNetCore.Identity;

namespace image_storage_API.Models
{
    /// <summary>
    /// Modelo de usuario para autenticación
    /// </summary>
    public class UserModel
    {
        public int IdUser { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
