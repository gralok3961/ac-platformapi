using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Base.Services.HttpCallRequest
{
    public class HttpCallRequestService(IHttpContextAccessor httpContextAccessor, HttpClient httpClient, string domain = null) : IHttpCallRequestService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly HttpClient _httpClient = httpClient;
        private readonly string _domain = domain;

        public async Task<string> SendGetRequestAsync(string url, object parameters = null, bool noAuth = false, Dictionary<string, string> headers = null)
        {
            var queryString = parameters != null ? QueryStringFromObject(parameters) : string.Empty;

            ApplyRequestHeaders(noAuth, headers);

            var requestUrl = string.IsNullOrWhiteSpace(queryString)
                ? $"{_domain}/{url}"
                : $"{_domain}/{url}?{queryString}";

            var response = await _httpClient.GetAsync(requestUrl);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            return responseContent;
        }

        public async Task<string> SendPostRequestAsync(string url, object parameters = null, bool noAuth = false, Dictionary<string, string> headers = null)
        {
            ApplyRequestHeaders(noAuth, headers);

            var content = parameters != null
                ? new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json")
                : null;

            var response = await _httpClient.PostAsync($"{_domain}/{url}", content);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private static string QueryStringFromObject(dynamic obj)
        {
            var properties = obj.GetType().GetProperties();

            var queryString = new StringBuilder();

            foreach (var property in properties)
            {
                if (queryString.Length > 0)
                {
                    _ = queryString.Append("&");
                }

                queryString.Append($"{property.Name}={property.GetValue(obj)}");
            }

            return queryString.ToString();
        }

        private void ApplyRequestHeaders(bool noAuth, Dictionary<string, string> headers)
        {
            _httpClient.DefaultRequestHeaders.Clear();

            if (!noAuth)
            {
                var accessToken = _httpContextAccessor.HttpContext?.Request?.Headers.Authorization.ToString();

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new InvalidOperationException("Authorization token is missing.");
                }

                if (!accessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    accessToken = $"Bearer {accessToken}";
                }

                _httpClient.DefaultRequestHeaders.Add("Authorization", accessToken);
            }

            if (headers == null) return;

            foreach (var header in headers)
            {
                if (string.IsNullOrWhiteSpace(header.Key) || string.IsNullOrWhiteSpace(header.Value))
                    continue;

                _httpClient.DefaultRequestHeaders.Remove(header.Key);
                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }
    }
}
