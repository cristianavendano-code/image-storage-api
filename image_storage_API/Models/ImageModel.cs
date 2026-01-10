namespace image_storage_API.Models
{
    /// <summary>
    /// Modelo completo de imagen (incluye bytes)
    /// Usa solo para operaciones individuales
    /// </summary>
    public class ImageModel
    {
        public int IdImage { get; set; }
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ContentType {  get; set; } = string.Empty;
        public int FileSize { get; set; }
        public string? Description { get; set; }
        public bool IsPrivate { get; set; }
        public int IdUser { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
