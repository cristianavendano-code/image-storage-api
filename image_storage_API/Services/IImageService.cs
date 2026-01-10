using image_storage_API.Models;
using image_storage_API.Models.DTOs;

namespace image_storage_API.Services
{
    /// <summary>
    /// Contrato que define operaciones disponibles para imagenes.
    /// El Controller dependerá de esta interfaz, no de la implementación.
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Obtiene todas las imagenes publicas
        /// </summary>
        List<ImageListDTO> GetPublicImages(int page = 1, int pageSize = 20);

        /// <summary>
        /// Obtiene todas las imagenes de un usuario especifico
        /// </summary>
        List<ImageListDTO> GetUserImages(int userId, bool includePrivate = false);

        /// <summary>
        /// Obtiene una imagen completa por ID (con bytes)
        /// </summary>
        ImageModel? GetImageById(int id);

        /// <summary>
        /// Sube una nueva imagen
        /// </summary>
        ImageModel UploadImage(IFormFile file, string? description, bool isPrivate, int userId);

        /// <summary>
        /// Actualiza descripción o privacidad de una imagen
        /// </summary>
        ImageModel? UpdateImage(int id, UpdateImageDTO dto, int userId);

        /// <summary>
        /// Elimina una imagen (solo el dueño)
        /// </summary>
        bool DeleteImage(int id, int userId);

        /// <summary>
        /// Verifica si un usuario es dueño de una imagen
        /// </summary>
        bool IsOwner(int imageId, int userId);
    }
}