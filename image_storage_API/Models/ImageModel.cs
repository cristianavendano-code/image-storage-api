namespace image_storage_API.Models
{
    /// <summary>
    /// Modelo de imagen
    /// </summary>

    public class ImageModel
    {
        public int IdImage { get; set; }

        // Imagen como binario
        public required byte[] Image { get; set; }

        public string? Description { get; set; }

        public required bool Private { get; set; }

        public int IdUser { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}