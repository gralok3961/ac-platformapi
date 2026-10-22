using System.Net;

namespace Base.Errors
{
    public class ApiException : ApiResponse
    {
        public string Details { get; set; }

        public ApiException(HttpStatusCode statusCode, string message = null, string details = null) : base(statusCode, message)
        {
            Details = details;
        }
    }
}
