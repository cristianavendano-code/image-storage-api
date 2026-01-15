using image_storage_API.Exceptions;
using image_storage_API.Models;
using image_storage_API.Models.DTOs;
using MySql.Data.MySqlClient;

namespace image_storage_API.Services
{
    public class ImageService : IImageService
    {
        private readonly string _connectionString;
        private readonly ILogger<ImageService> _logger;

        private const int MAX_FILE_SIZE = 5 * 1024 * 1024;  // 5MB
        private static readonly string[] ALLOWED_TYPES = { "image/jpeg", "image/png", "image/gif", "image/webp" };

        public ImageService(IConfiguration configuration, ILogger<ImageService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string no encontrada");
            _logger = logger;
        }

        // Para obtener toda las imagenes publicas
        public List<ImageListDTO> GetPublicImages(int page = 1, int pageSize = 20)
        {
            try
            {
                _logger.LogInformation("Obteniendo imágenes públicas. Página: {Page}, Tamaño: {PageSize}", page, pageSize);

                var images = new List<ImageListDTO>();
                int offset = (page - 1) * pageSize;

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    var command = new MySqlCommand(
                        "SELECT id_image, filename, description, file_size, is_private, id_user, created_at " +
                        "FROM images " +
                        "WHERE is_private = 0 " +
                        "ORDER BY created_at DESC " +
                        "LIMIT @pageSize OFFSET @offset",
                        connection
                        );

                    command.Parameters.AddWithValue("@pageSize", pageSize);
                    command.Parameters.AddWithValue("@offset", offset);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            images.Add(new ImageListDTO
                            {
                                IdImage = reader.GetInt32("id_image"),
                                Filename = reader.GetString("filename"),
                                Description = reader.IsDBNull(reader.GetOrdinal("description"))
                                    ? null
                                    : reader.GetString("description"),
                                FileSize = reader.GetInt32("file_size"),
                                IsPrivate = reader.GetBoolean("is_private"),
                                IdUser = reader.GetInt32("id_user"),
                                CreatedAt = reader.GetDateTime("created_at")
                            });
                        }
                    }
                }

                _logger.LogInformation("Se obtuvieron {Count} imágenes públicas", images.Count);
                return images;
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de BD al obtener imágenes públicas");
                throw new DatabaseException("Error al obtener las imágenes", ex);
            }
        }

        // Para obtener todas las imagenes del usuario autenticado (publicas y privadas)
        public List<ImageListDTO> GetUserImages(int userId, bool includePrivate = false)
        {
            try
            {
                _logger.LogInformation("Obteniendo imágenes del usuario {UserId}, includePrivate: {IncludePrivate}",
                     userId, includePrivate);

                var images = new List<ImageListDTO>();

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = includePrivate
                        ? "SELECT id_image, filename, description, file_size, is_private, id_user, created_at " +
                          "FROM images WHERE id_user = @userId ORDER BY created_at DESC"
                        : "SELECT id_image, filename, description, file_size, is_private, id_user, created_at " +
                          "FROM images WHERE id_user = @userId AND is_private = 0 ORDER BY created_at DESC";

                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            images.Add(new ImageListDTO
                            {
                                IdImage = reader.GetInt32("id_image"),
                                Filename = reader.GetString("filename"),
                                Description = reader.IsDBNull(reader.GetOrdinal("description"))
                                    ? null
                                    : reader.GetString("description"),
                                FileSize = reader.GetInt32("file_size"),
                                IsPrivate = reader.GetBoolean("is_private"),
                                IdUser = reader.GetInt32("id_user"),
                                CreatedAt = reader.GetDateTime("created_at")
                            });
                        }
                    }
                }

                _logger.LogInformation("Usuario {UserId} tiene {Count} imágenes", userId, images.Count);
                return images;
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de BD al obtener imágenes del usuario {UserId}", userId);
                throw new DatabaseException("Error al obtener tus imágenes", ex);
            }
        }

        public ImageModel? GetImageById(int id)
        {
            try
            {
                _logger.LogInformation("Obteniendo imagen completa con ID: {IdImage}", id);

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    var command = new MySqlCommand(
                        "SELECT id_image, image_data, filename, content_type, file_size, description, " +
                        "is_private, id_user, created_at, updated_at " +
                        "FROM images WHERE id_image = @id",
                        connection
                    );

                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var image = new ImageModel
                            {
                                IdImage = reader.GetInt32("id_image"),
                                ImageData = GetBytesFromReader(reader, "image_data"),
                                FileName = reader.GetString("filename"),
                                ContentType = reader.GetString("content_type"),
                                FileSize = reader.GetInt32("file_size"),
                                Description = reader.IsDBNull(reader.GetOrdinal("description"))
                                    ? null
                                    : reader.GetString("description"),
                                IsPrivate = reader.GetBoolean("is_private"),
                                IdUser = reader.GetInt32("id_user"),
                                CreatedAt = reader.GetDateTime("created_at"),
                                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at"))
                                    ? null
                                    : reader.GetDateTime("updated_at")
                            };

                            _logger.LogInformation("Imagen {IdImage} encontrada. Tamaño: {Size} bytes", id, image.FileSize);
                            return image;
                        }
                    }
                }

                _logger.LogWarning("Imagen con ID {IdImage} no encontrada", id);
                return null;
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de BD al buscar imagen {IdImage}", id);
                throw new DatabaseException($"Error al buscar la imagen con ID {id}", ex);
            }
        }


        public ImageModel UploadImage(IFormFile file, string? description, bool isPrivate, int userId)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new ValidationException("No se proporcionó ninguna imagen");
                }

                if (file.Length > MAX_FILE_SIZE)
                {
                    throw new ValidationException($"La imagen no puede superar {MAX_FILE_SIZE / 1024 / 1024}MB");
                }

                if (!ALLOWED_TYPES.Contains(file.ContentType.ToLower()))
                {
                    throw new ValidationException($"Tipo de archivo no permitido. Usa: {string.Join(", ", ALLOWED_TYPES)}");
                }
                _logger.LogInformation("Subiendo imagen. Usuario: {UserId}, Tamaño: {Size} bytes, Tipo: {ContentType}, Privada: {IsPrivate}", userId, file.Length, file.ContentType, isPrivate);

                //leer bytes de la imagen
                byte[] imageBytes;

                using (var memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }

                //insertar en bd
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    var command = new MySqlCommand(
                        "INSERT INTO images (image_data, filename, content_type, file_size, description, is_private, id_user, created_at) " +
                        "VALUES (@imageData, @filename, @contentType, @fileSize, @description, @isPrivate, @idUser, @createdAt); " +
                        "SELECT LAST_INSERT_ID();",
                        connection
                        );

                    command.Parameters.AddWithValue("@imageData", imageBytes);
                    command.Parameters.AddWithValue("@filename", file.FileName);
                    command.Parameters.AddWithValue("@contentType", file.ContentType);
                    command.Parameters.AddWithValue("@fileSize", file.Length);
                    command.Parameters.AddWithValue("@description", description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@isPrivate", isPrivate);
                    command.Parameters.AddWithValue("@idUser", userId);
                    command.Parameters.AddWithValue("@createdAt", DateTime.Now);

                    int newId = Convert.ToInt32(command.ExecuteScalar());

                    _logger.LogInformation("Imagen subida exitosamente con ID: {IdImage}", newId);

                    return new ImageModel
                    {
                        IdImage = newId,
                        ImageData = imageBytes,
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        FileSize = (int)file.Length,
                        Description = description,
                        IsPrivate = isPrivate,
                        IdUser = userId,
                        CreatedAt = DateTime.Now
                    };
                }
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de BD al subir imagen");
                throw new DatabaseException("Error al guardar la imagen", ex);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error inesperado al subir imagen");
                throw new DatabaseException("Error interno al procesar la imagen", ex);
            }
        }

        public ImageModel? UpdateImage(int id, UpdateImageDTO dto, int userId)
            {
                try
                {
                    _logger.LogInformation("Actualizando imagen {IdImage} por usuario {UserId}", id, userId);

                    // Verificar que existe y pertenece al usuario
                    var existingImage = GetImageById(id);
                    if (existingImage == null)
                    {
                        _logger.LogWarning("Intento de actualizar imagen inexistente: {IdImage}", id);
                        return null;
                    }

                    if (existingImage.IdUser != userId)
                    {
                        _logger.LogWarning("Usuario {UserId} intentó actualizar imagen {IdImage} que no le pertenece", userId, id);
                        throw new ValidationException("No tienes permiso para modificar esta imagen");
                    }

                    // Preparar valores
                    var newDescription = dto.Description ?? existingImage.Description;
                    var newIsPrivate = dto.IsPrivate ?? existingImage.IsPrivate;

                    // Actualizar en BD
                    using (var connection = new MySqlConnection(_connectionString))
                    {
                        connection.Open();

                        var command = new MySqlCommand(
                            "UPDATE images SET description = @description, is_private = @isPrivate, updated_at = @updatedAt " +
                            "WHERE id_image = @id",
                            connection
                        );

                        command.Parameters.AddWithValue("@description", newDescription ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@isPrivate", newIsPrivate);
                        command.Parameters.AddWithValue("@updatedAt", DateTime.Now);
                        command.Parameters.AddWithValue("@id", id);

                        command.ExecuteNonQuery();
                    }

                    _logger.LogInformation("Imagen {IdImage} actualizada exitosamente", id);
                    return GetImageById(id);
                }
                catch (ValidationException)
                {
                    throw;
                }
                catch (MySqlException ex)
                {
                    _logger.LogError(ex, "Error de BD al actualizar imagen {IdImage}", id);
                    throw new DatabaseException($"Error al actualizar la imagen {id}", ex);
                }
            }

        public bool DeleteImage(int id, int userId)
        {
            try
            {
                _logger.LogInformation("Eliminando imagen {IdImage} por usuario {UserId}", id, userId);

                // Verificar que existe y pertenece al usuario
                var image = GetImageById(id);
                if (image == null)
                {
                    _logger.LogWarning("Intento de eliminar imagen inexistente: {IdImage}", id);
                    return false;
                }

                if (image.IdUser != userId)
                {
                    _logger.LogWarning("Usuario {UserId} intentó eliminar imagen {IdImage} que no le pertenece", userId, id);
                    throw new ValidationException("No tienes permiso para eliminar esta imagen");
                }

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    var command = new MySqlCommand(
                        "DELETE FROM images WHERE id_image = @id",
                        connection
                    );

                    command.Parameters.AddWithValue("@id", id);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        _logger.LogInformation("Imagen {IdImage} eliminada exitosamente", id);
                        return true;
                    }
                }

                return false;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de BD al eliminar imagen {IdImage}", id);
                throw new DatabaseException($"Error al eliminar la imagen {id}", ex);
            }
        }

        // Helper: verificar propiedades
        public bool IsOwner(int imageId, int userId)
        {
            var image = GetImageById(imageId);
            return image != null && image.IdUser == userId;
        }

        // Helper: Leer bytes del reader
        private byte[] GetBytesFromReader(MySqlDataReader reader,  string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);

            if (reader.IsDBNull(ordinal))
            {
                return Array.Empty<byte>();
            }

            return (byte[])reader[columnName];
        }
    }
}
