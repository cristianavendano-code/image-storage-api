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
        List<ImageModel> GetAllPublicImages();

        /// <summary>
        /// Obtiene todas las imagenes del usuario (privadas y publicas)
        /// </summary>
        List<ImageModel> GetAllUserImages();

        /// <summary>
        /// Obtiene una imagen del usuario por ID
        /// </summary>
        /// <param name="id">ID de la imagen</param>
        /// <returns>ImageModel si existe, null si no</returns>
        ImageModel? GetImageById(int id);

        /// <summary>
        /// Crea una nueva imagen
        /// </summary>
        /// <param name="dto">Datos de la imagen a crear</param>
        /// <returns>Imagen creada con ID asignado</returns>
        ImageModel Create(CreateImageDTO dto);

        /// <summary>
        /// Actualiza las propiedades de una imagen existente
        /// </summary>
        /// <param name="id">ID de la imagen</param>
        /// <param name="dto">Datos a actualizar</param>
        /// <returns>Imagen actualizada si existe, null si no</returns>
        ImageModel? Update(int id, UpdateImageDTO dto);

        /// <summary>
        /// Elimina una imagen
        /// </summary>
        /// <param name="id">ID de la imagen</param>
        /// <returns>true si se eliminó, false si no existía</returns>
        bool Delete(int id);
    }
}