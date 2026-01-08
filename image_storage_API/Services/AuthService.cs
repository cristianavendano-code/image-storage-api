using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using image_storage_API.Models;
using image_storage_API.Models.DTOs;
using image_storage_API.Exceptions;

namespace image_storage_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string no encontrada");
            _configuration = configuration;
            _logger = logger;
        }

        public string? Login(LoginDTO dto)
        {
            try
            {
                _logger.LogInformation("Intento de login para usuario: {Username}", dto.Username);

                // PASO 1: Buscar usuario en BD
                UserModel? user = GetUserByUsername(dto.Username);

                if (user == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {Username}", dto.Username);
                    return null;
                }

                // PASO 2: Verificar password
                // TEMPORAL: Comparación directa (sin hash)
                // En producción: BCrypt.Verify(dto.Password, user.PasswordHash)
                if (user.PasswordHash != dto.Password)
                {
                    _logger.LogWarning("Password incorrecto para usuario: {Username}", dto.Username);
                    return null;
                }

                // PASO 3: Generar JWT
                string token = GenerateJwtToken(user);

                _logger.LogInformation("Login exitoso para usuario: {Username}", dto.Username);
                return token;
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de BD en login");
                throw new DatabaseException("Error al procesar login", ex);
            }
        }

        public UserModel? Register(RegisterDTO dto)
        {
            try
            {
                _logger.LogInformation("Registrando nuevo usuario: {Username}", dto.Username);

                // PASO 1: Verificar que no exista
                if (UserExists(dto.Username))
                {
                    _logger.LogWarning("Username ya existe: {Username}", dto.Username);
                    return null;
                }

                // PASO 2: Insertar usuario
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    var command = new MySqlCommand(
                        "INSERT INTO users (username, email, passwordHash, createdAt) " +
                        "VALUES (@username, @email, @password, @createdAt); " +
                        "SELECT LAST_INSERT_ID();",
                        connection
                    );

                    command.Parameters.AddWithValue("@username", dto.Username);
                    command.Parameters.AddWithValue("@email", dto.Email);
                    // TEMPORAL: Guardamos password sin hashear
                    // En producción: BCrypt.HashPassword(dto.Password
                    command.Parameters.AddWithValue("@password", dto.Password);
                    command.Parameters.AddWithValue("@createdAt", DateTime.Now);

                    int newId = Convert.ToInt32(command.ExecuteScalar());

                    _logger.LogInformation("Usuario registrado con ID: {UserId}", newId);

                    return new UserModel
                    {
                        IdUser = newId,
                        Username = dto.Username,
                        Email = dto.Email,
                        CreatedAt = DateTime.Now
                    };
                }
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de BD en registro");
                throw new DatabaseException("Error al registrar usuario", ex);
            }
        }

        public bool UserExists(string username)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var command = new MySqlCommand(
                    "SELECT COUNT(*) FROM users WHERE username = @username",
                    connection
                );

                command.Parameters.AddWithValue("@username", username);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }

        // ========== MÉTODOS PRIVADOS ==========

        private UserModel? GetUserByUsername(string username)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var command = new MySqlCommand(
                    "SELECT idUser, username, email, passwordHash, createdAt " +
                    "FROM users WHERE username = @username",
                    connection
                );

                command.Parameters.AddWithValue("@username", username);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new UserModel
                        {
                            IdUser = reader.GetInt32("idUser"),
                            Username = reader.GetString("username"),
                            PasswordHash = reader.GetString("passwordHash"),
                            Email = reader.GetString("email"),
                            CreatedAt = reader.GetDateTime("createdAt")
                        };
                    }
                }
            }

            return null;
        }

        private string GenerateJwtToken(UserModel user)
        {
            // PASO 1: Leer configuración JWT desde appsettings.json
            var secret = _configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("JWT Secret no configurado");
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

            // PASO 2: Crear claims (datos que irán dentro del token)
            var claims = new[]
            {
                // ClaimTypes.NameIdentifier = userId (lo usarás para saber quién es)
                new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                // JTI = identificador único del token
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // PASO 3: Crear llave de firma (usando el secret de appsettings)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // PASO 4: Crear el token JWT
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            // PASO 5: Convertir el token a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}