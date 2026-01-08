using System.ComponentModel.DataAnnotations;

namespace image_storage_API.Models.DTOs
{
    /// <summary>
    /// DTO para CREAR imagenes
    /// Solo contiene lo que el cliente debe enviar
    /// </summary>
    public class CreateImageDTO
    {
        [Required(ErrorMessage = "La imagen es obligatoria")]
        public required byte[] Image { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "true si la imagen es privada, false si la imagen es publica")]
        public required bool Private { get; set; }
    }
}
