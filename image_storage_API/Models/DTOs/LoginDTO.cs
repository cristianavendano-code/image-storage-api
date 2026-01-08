using System.ComponentModel.DataAnnotations;

namespace image_storage_API.Models.DTOs
{
    /// <summary>
    /// DTO para login de usuario
    /// </summary>
    public class LoginDTO
    {
        [Required(ErrorMessage = "El username es obligatorio")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "El password es obligatorio")]
        [MinLength(6, ErrorMessage = "El password debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}
