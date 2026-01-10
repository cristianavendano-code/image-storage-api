namespace image_storage_API.Models.DTOs
{
    /// <summary>
    /// DTO ligero para listar imágenes (galería estilo Pinterest)
    /// NO incluye los bytes de la imagen
    /// </summary>
    public class ImageListDTO
    {
        public int IdImage { get; set; }
        public string Filename { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int FileSize { get; set; }
        public string FileSizeFormatted => FormatFileSize(FileSize);
        public bool IsPrivate { get; set; }
        public int IdUser { get; set; }
        public DateTime CreatedAt { get; set; }

        //URL para obtener la imagen completa
        public string ImageUrl => $"/api/images.{IdImage}";

        //URL para obtener thumbnail (implementacion futura)
        public string ThumbnailUrl => $"/api/images/{IdImage}/thumbnail";

        private string FormatFileSize(int bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024} KB";
            return $"{bytes / (1024 * 1024)} MB";
        }

    }
}
