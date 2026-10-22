using System.Net;

namespace Base.Errors
{
    public class ApiResponse(HttpStatusCode statusCode, string message = null)
    {
        public HttpStatusCode StatusCode { get; set; } = statusCode;

        public string Message { get; set; } = message ?? GetDefaultMessageForStatusCode(statusCode);

        private static string GetDefaultMessageForStatusCode(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.BadRequest => "Bad Request",
                HttpStatusCode.Unauthorized => "You are not authorized",
                HttpStatusCode.Forbidden => "Forbidden",
                HttpStatusCode.NotFound => "No Resource Found",
                HttpStatusCode.InternalServerError => "Server error contact Administrator",
                _ => null
            };
        }
    } // end ApiResponse class

}
