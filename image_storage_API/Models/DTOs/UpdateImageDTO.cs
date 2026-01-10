using System.ComponentModel.DataAnnotations;

namespace image_storage_API.Models.DTOs
{
    /// <summary>
    /// DTO para actualizar descripción o privacidad de imagen
    /// NO permite cambiar la imagen en sí
    /// </summary>
    public class UpdateImageDTO
    {
        [StringLength(500,ErrorMessage = "Descripción máximo 500 caracteres")]
        public string? Description { get; set; }
        public bool? IsPrivate { get; set; }
    }
}
