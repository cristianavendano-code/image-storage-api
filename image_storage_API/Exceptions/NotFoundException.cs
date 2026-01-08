namespace image_storage_API.Exceptions
{
    /// <summary>
    /// Excepción personalizada para recursos no encontrados
    /// Se mapea a HTTP 404
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
