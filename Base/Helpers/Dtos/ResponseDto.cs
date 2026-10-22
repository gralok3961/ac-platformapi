using System.Net;

namespace Base.Helpers.Dtos
{
    public class ResponseDto<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public ResponseDto()
        {
        }

        public ResponseDto(T data = default)
        {
            Data = data;
        }
        public ResponseDto(HttpStatusCode statusCode = 0, string message = null)
        {
            StatusCode = statusCode;

            Message = message;
        }
    }
}
