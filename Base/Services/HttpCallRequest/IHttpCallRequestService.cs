namespace Base.Services.HttpCallRequest
{
    public interface IHttpCallRequestService
    {
        Task<string> SendGetRequestAsync(string url, object? parameters = null, bool noAuth = false, Dictionary<string, string>? headers = null);

        Task<string> SendPostRequestAsync(string url, object? parameters = null, bool noAuth = false, Dictionary<string, string>? headers = null);
    }
}
