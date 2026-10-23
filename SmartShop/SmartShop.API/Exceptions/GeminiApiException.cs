using System.Net;

namespace SmartShop.API.Exceptions;

public class GeminiApiException : Exception
{
    public HttpStatusCode? StatusCode { get; }

    public GeminiApiException(string message, HttpStatusCode? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
