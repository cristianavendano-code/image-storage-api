using image_storage_API.Exceptions;
using image_storage_API.Models;
using image_storage_API.Models.DTOs;
using image_storage_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace image_storage_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly ILogger<ImageController> _logger;

        public ImageController(IImageService imageService, ILogger<ImageController> logger)
        {
            _imageService = imageService;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateImageDTO dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var idUser))
                {
                    _logger.LogWarning("User id missing or invalid in token");
                    return Unauthorized(new { error = "User not authenticated" });
                }

                _logger.LogInformation("Creando imagen. UserId: {UserId}", idUser);

                var newImage = _imageService.Create(dto, idUser);

                return Ok(newImage);
            }
            catch (DatabaseException ex)
            {
                _logger.LogError(ex, "Error de BD al crear imagen");
                return StatusCode(500, new
                {
                    error = "Error al crear la imagen",
                    message = "Por favor, intente más tarde",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating image");
                return StatusCode(500, new
                {
                    error = "Error interno",
                    message = "Por favor, intente más tarde",
                    timestamp = DateTime.Now
                });
            }
        }
    }
}
