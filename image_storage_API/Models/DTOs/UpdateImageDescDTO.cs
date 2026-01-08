using System.ComponentModel.DataAnnotations;

namespace image_storage_API.Models.DTOs
{
    /// <summary>
    /// DTO para ACTUALIZAR descripci[on y privacidad de las Imagenes
    /// </summary>
    public class UpdateImageDTO
    {
        public string? Description { get; set; } //? Nullable: opcional
        public bool? Private { get; set; }
    }
}
