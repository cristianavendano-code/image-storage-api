using System.ComponentModel.DataAnnotations;

namespace image_storage_API.Models.DTOs
{
    /// <summary>
    /// DTO para registro de usuario
    /// </summary>
    public class RegisterDTO
    {
        [Required(ErrorMessage = "El username es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username debe tener entre 3 y 50 caracteres")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email invalido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El password es obligatorio")]
        [MinLength(6, ErrorMessage = "Passsword debe tener mínimo 6 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}
