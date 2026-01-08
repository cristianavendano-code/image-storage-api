namespace image_storage_API.Exceptions
{
    /// <summary>
    /// Excepción para errores de validación de negocio
    /// Se mapea a HTTP 400
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {
        }
    }
}
