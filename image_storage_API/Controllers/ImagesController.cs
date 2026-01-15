using image_storage_API.Exceptions;
using image_storage_API.Models.DTOs;
using image_storage_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace image_storage_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(IImageService imageService, ILogger<ImagesController> logger)
        {
            _imageService = imageService;
            _logger = logger;
        }

        //ENDPOINTS PUBLICAS

        /// <summary>
        /// GET /api/images?page=1&pageSize=20
        /// Obtiene galería pública (estilo Pinterest)
        /// NO requiere autenticación
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetPublicGallery([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                // Validar parámetros
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                _logger.LogInformation("Obteniendo galería pública. Página: {Page}", page);

                var images = _imageService.GetPublicImages(page, pageSize);

                return Ok(new
                {
                    data = images,
                    page = page,
                    pageSize = pageSize,
                    count = images.Count
                });
            }
            catch (DatabaseException)
            {
                return StatusCode(500, new
                {
                    error = "Error al obtener la galería",
                    message = "Por favor, intente más tarde",
                    timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// GET /api/images/{id}
        /// Obtiene una imagen específica (pública o privada si eres el dueño)
        /// </summary>

        [HttpGet("{id}")]
        public IActionResult GetImage(int id)
        {
            try
            {
                var image = _imageService.GetImageById(id);

                if (image == null)
                {
                    return NotFound(new
                    {
                        error = $"Imagen con ID {id} no encontrada",
                        timestamp = DateTime.Now
                    });
                }

                // Si la imagen es privada, verifica que sea el dueño
                if (image.IsPrivate)
                {
                    // Obtener userId del token (si existe)
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (userIdClaim == null || int.Parse(userIdClaim) != image.IdUser)
                    {
                        _logger.LogWarning(
                            "Usuario {UserId} intentó acceder a imagen privada {IdImage}",
                            userIdClaim ?? "anónimo", id
                        );

                        return StatusCode(403, new
                        {
                            error = "Esta imagen es privada",
                            message = "No tienes permiso para verla",
                            timestamp = DateTime.Now
                        });
                    }
                }

                return File(image.ImageData, image.ContentType, image.FileName);
            }
            catch (DatabaseException)
            {
                return StatusCode(500, new
                {
                    error = "Error al obtener la imagen",
                    message = "Por favor, intente más tarde",
                    timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// GET /api/images/{id}/info
        /// Obtiene metadata de la imagen (sin los bytes)
        /// </summary>
        [HttpGet("{id}/info")]
        public IActionResult GetImageInfo(int id)
        {
            try
            {
                var image = _imageService.GetImageById(id);

                if (image == null)
                {
                    return NotFound(new
                    {
                        error = $"Imagen con ID {id} no encontrada",
                        timestamp = DateTime.Now
                    });
                }

                // Verificar privacidad
                if (image.IsPrivate)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (userIdClaim == null || int.Parse(userIdClaim) != image.IdUser)
                    {
                        return StatusCode(403, new
                        {
                            error = "Esta imagen es privada",
                            timestamp = DateTime.Now
                        });
                    }
                }

                // Devolver solo metadata (sin bytes)
                return Ok(new
                {
                    idImage = image.IdImage,
                    filename = image.FileName,
                    contentType = image.ContentType,
                    fileSize = image.FileSize,
                    description = image.Description,
                    isPrivate = image.IsPrivate,
                    idUser = image.IdUser,
                    createdAt = image.CreatedAt,
                    updatedAt = image.UpdatedAt,
                    imageUrl = $"/api/images/{image.IdImage}"
                });
            }
            catch (DatabaseException)
            {
                return StatusCode(500, new
                {
                    error = "Error al obtener información de la imagen",
                    message = "Por favor, intente más tarde",
                    timestamp = DateTime.Now
                });
            }
        }

        //ENDPOINTS Autenticados

        /// <summary>
        /// GET /api/images/my-images
        /// Obtiene todas las imágenes del usuario autenticado
        /// </summary>
        [Authorize]
        [HttpGet("my-images")]
        public IActionResult GetMyImages()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());

                _logger.LogInformation("Usuario {UserId} obteniendo sus imágenes", userId);

                var images = _imageService.GetUserImages(userId, includePrivate: true);

                return Ok(new
                {
                    data = images,
                    count = images.Count,
                    publicCount = images.Count(i => !i.IsPrivate),
                    privateCount = images.Count(i => i.IsPrivate)
                });
            }
            catch (DatabaseException)
            {
                return StatusCode(500, new
                {
                    error = "Error al obtener tus imágenes",
                    message = "Por favor, intente más tarde",
                    timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// POST /api/images
        /// Sube una nueva imagen
        /// Content-Type: multipart/form-data
        /// </summary>
        [Authorize]
        [HttpPost]
        public IActionResult UploadImage([FromForm] CreateImageDTO dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? throw new UnauthorizedAccessException());
                var username = User.FindFirst(ClaimTypes.Name)?.Value;

                _logger.LogInformation(
                    "Usuario {Username} (ID: {UserId}) subiendo imagen: {Filename}",
                    username, userId, dto.Image?.FileName
                );

                var uploadedImage = _imageService.UploadImage(
                    dto.Image,
                    dto.Description,
                    dto.IsPrivate,
                    userId
                );

                // Devolver metadata (sin bytes)
                return CreatedAtAction(
                    nameof(GetImage),
                    new { id = uploadedImage.IdImage },
                    new
                    {
                        idImage = uploadedImage.IdImage,
                        filename = uploadedImage.FileName,
                        fileSize = uploadedImage.FileSize,
                        description = uploadedImage.Description,
                        isPrivate = uploadedImage.IsPrivate,
                        createdAt = uploadedImage.CreatedAt,
                        imageUrl = $"/api/images/{uploadedImage.IdImage}",
                        message = "Imagen subida exitosamente"
                    }
                );
            }
            catch (ValidationException ex)
            {
                return BadRequest(new
                {
                    error = ex.Message,
                    timestamp = DateTime.Now
                });
            }
            catch (DatabaseException)
            {
                return StatusCode(500, new
                {
                    error = "Error al subir la imagen",
                    message = "Por favor, intente más tarde",
                    timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// PUT /api/images/{id}
        /// Actualiza descripción o privacidad de una imagen
        /// Solo el dueño puede hacerlo
        /// </summary>
        [Authorize]
        [HttpPut("{id}")]
        public IActionResult UpdateImage(int id, [FromBody] UpdateImageDTO dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? throw new UnauthorizedAccessException());

                _logger.LogInformation("Usuario {UserId} actualizando imagen {IdImage}", userId, id);

                var updatedImage = _imageService.UpdateImage(id, dto, userId);

                if (updatedImage == null)
                {
                    return NotFound(new
                    {
                        error = $"Imagen con ID {id} no encontrada",
                        timestamp = DateTime.Now
                    });
                }

                // Devolver metadata actualizada (sin bytes)
                return Ok(new
                {
                    idImage = updatedImage.IdImage,
                    filename = updatedImage.FileName,
                    description = updatedImage.Description,
                    isPrivate = updatedImage.IsPrivate,
                    updatedAt = updatedImage.UpdatedAt,
                    message = "Imagen actualizada exitosamente"
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new
                {
                    error = ex.Message,
                    timestamp = DateTime.Now
                });
            }
            catch (DatabaseException)
            {
                return StatusCode(500, new
                {
                    error = "Error al actualizar la imagen",
                    message = "Por favor, intente más tarde",
                    timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// DELETE /api/images/{id}
        /// Elimina una imagen
        /// Solo el dueño puede hacerlo
        /// </summary>
        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult DeleteImage(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? throw new UnauthorizedAccessException());

                _logger.LogInformation("Usuario {UserId} eliminando imagen {IdImage}", userId, id);

                bool deleted = _imageService.DeleteImage(id, userId);

                if (!deleted)
                {
                    return NotFound(new
                    {
                        error = $"Imagen con ID {id} no encontrada",
                        timestamp = DateTime.Now
                    });
                }

                return NoContent();
            }
            catch (ValidationException ex)
            {
                return StatusCode(403, new
                {
                    error = ex.Message,
                    timestamp = DateTime.Now
                });
            }
            catch (DatabaseException)
            {
                return StatusCode(500, new
                {
                    error = "Error al eliminar la imagen",
                    message = "Por favor, intente más tarde",
                    timestamp = DateTime.Now
                });
            }
        }
    }
}
