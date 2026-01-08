using image_storage_API.Exceptions;
using image_storage_API.Models.DTOs;
using image_storage_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace image_storage_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        ///<summary>
        /// POST /api/auth/login
        /// Autentica un usuario y devuelve JWT
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO dto)
        {
            try
            {
                string? token = _authService.Login(dto);

                if (token == null)
                {
                    return Unauthorized(new
                    {
                        error = "Credenciales inválidas",
                        message = "Usuario o password incorrectos",
                        timestamp = DateTime.Now
                    });
                }

                return Ok(new
                {
                    token = token,
                    expiresIn = 3600,
                    tokenType = "Bearer"
                });
            }
            catch (DatabaseException)
            {
                return StatusCode(500, new
                {
                    error = "Error al procesar login",
                    message = "Por favor, intente más tarde",
                    timestamp = DateTime.Now
                });
            }
        }
        /// <summary>
        /// POST /api/auth/register
        /// Regista un nuevo usuario
        /// </summary>
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDTO dto)
        {
            try
            {
                var user = _authService.Register(dto);

                if (user == null)
                {
                    return Conflict(new
                    {
                        error = "El username ya existe",
                        message = "Intenta con otro username",
                        timestamp = DateTime.Now
                    });
                }

                return CreatedAtAction(nameof(Login), null, new
                {
                    message = "Usuario registrado exitosamente",
                    userId = user.IdUser,
                    username = user.Username,
                    email = user.Email
                });
            }
            catch (DatabaseException)
            {
                return StatusCode(500, new
                {
                    error = "Error al registrar usuario",
                    message = "Por favor, intente más tarde",
                    timestamp = DateTime.Now
                });
            }
        }
    }
}