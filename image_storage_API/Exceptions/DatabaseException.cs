namespace image_storage_API.Exceptions
{
    /// <summary>
    /// Excepción de errores de base de datos
    /// Se mapea a HTTP 500
    /// </summary>
    public class DatabaseException : Exception
    {
        public DatabaseException(string message) : base(message)
        {
        }

        public DatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
