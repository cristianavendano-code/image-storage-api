using System.ComponentModel.DataAnnotations;

namespace image_storage_API.Models.DTOs
{
    /// <summary>
    /// DTO para subir una imagen
    /// </summary>
    public class CreateImageDTO
    {
        [Required(ErrorMessage = "La imagen es obligatoria")]
        public IFormFile Image { get; set; } = null!; //IFormFile para multipart/form-data

        [StringLength(500, ErrorMessage = "Descripción máximo 500 caracteres")]
        public string? Description { get; set; }

        public bool IsPrivate { get; set; } = false; //Publica por defecto
    }
}
