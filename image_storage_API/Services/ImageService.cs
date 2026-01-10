using image_storage_API.Exceptions;
using image_storage_API.Models;
using image_storage_API.Models.DTOs;
using MySql.Data.MySqlClient;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace image_storage_API.Services
{
    public class ImageService : IImageService
    {
        private readonly string _connectionString;
        private readonly ILogger<ImageService> _logger;

        private const int MAX_IMAGE_SIZE = 5 * 1024 * 1024;  // 5MB

        public ImageService(IConfiguration configuration, ILogger<ImageService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string no encontrada");
            _logger = logger;
        }

        public List<ImageModel> GetAllPublicImages()
        {
            //    try
            //    {
            //        _logger.LogInformation("Obteniendo todas las imagenes");

            //        var images = new List<ImageModel>();

            //        using (var connection = new MySqlConnection(_connectionString))
            //        {
            //            connection.Open();

            //            var command = new MySqlCommand(
            //                "SELECT idImage, image, description, isPrivate, idUser, createdAt FROM images WHERE isPrivate = 0 ORDER BY createdAt DESC",
            //                connection
            //                );

            //            using (var reader = command.ExecuteReader())
            //            {
            //                // Leer cada fila de resultados
            //                while (reader.Read())
            //                {
            //                    images.Add(new ImageModel
            //                    {
            //                        IdImage = reader.GetInt32("idImage"),
            //                        Image = Bytes(reader, "image"),
            //                        Description = reader.GetString("description"),
            //                        IsPrivate = reader.GetBoolean("isPrivate"),
            //                        IdUser = reader.GetInt32("idUser"),
            //                        CreatedAt = reader.GetDateTime("createdAt")
            //                    });
            //                }
            //            }
            //        }
            //        _logger.LogInformation("Se obtuvieron {Count} Imagenes", images.Count);
            //        return images;
            //    }
            //    catch (MySqlException ex)
            //    {
            //        _logger.LogError(ex, "Error de base de datos al obtener imagenes");

            //        throw new DatabaseException("Error al obtener las imagenes", ex);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogCritical(ex, "Error inesperado al obtener imagenes");
            //        throw new DatabaseException("Error interno al procesar la solicitud", ex);
            //    }
            throw new NotImplementedException();
        }

        public ImageModel Create(CreateImageDTO dto, int idUser)
        {
            try
            {

                if (dto.Image.Length > MAX_IMAGE_SIZE)
                {
                    _logger.LogWarning("Intento de subir imagen muy grande: {Size} bytes", dto.Image.Length);
                    throw new ValidationException($"La imagen no puede superar {MAX_IMAGE_SIZE / 1024 / 1024}MB");
                }

                _logger.LogInformation("Creando nueva imagen. Tamaño: {Size} bytes, IdUser: {IdUser}", dto.Image.Length, idUser);


                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    var command = new MySqlCommand(
                        "INSERT INTO images (image, description, isPrivate, idUser, createdAt) VALUES (@image, @description, @isPrivate, @idUser, @createdAt); SELECT LAST_INSERT_ID();",
                        connection
                        );

                    command.Parameters.AddWithValue("@image", dto.Image);
                    command.Parameters.AddWithValue("@description", dto.Description);
                    command.Parameters.AddWithValue("@isPrivate", dto.IsPrivate);
                    command.Parameters.AddWithValue("@idUser", idUser);
                    command.Parameters.AddWithValue("@createdAt", DateTime.Now);

                    var newId = Convert.ToInt32(command.ExecuteScalar());

                    var newImage = new ImageModel
                    {
                        IdImage = newId,
                        Image = dto.Image,
                        Description = dto.Description,
                        IsPrivate = dto.IsPrivate,
                        IdUser = idUser,
                        CreatedAt = DateTime.Now,
                    };

                    _logger.LogInformation("\"Imagen creada exitosamente con ID: {IdImage}", newId);
                    return newImage;
                }
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de BD al crear imagen");
                throw new DatabaseException("Error al crear la imagen", ex);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error inesperado al crear imagen");
                throw new DatabaseException("Error interno al procesar la solicitud", ex);
            }
        }

        // --- Stub implementations to satisfy IImageService while you only test Create() ---

        public List<ImageModel> GetAllUserImages(int idUser)
        {
            throw new NotImplementedException();
        }

        public ImageModel? GetImageById(int idImage, int idUser)
        {
            throw new NotImplementedException();
        }

        public ImageModel? Update(int idImage, UpdateImageDTO dto, int idUser)
        {
            throw new NotImplementedException();
        }

        public bool Delete(int idImage, int idUser)
        {
            throw new NotImplementedException();
        }
    }
}
