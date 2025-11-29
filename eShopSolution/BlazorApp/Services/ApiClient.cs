using BlazorApp.Interfaces;
using BlazorApp.Models;
using System.Buffers.Text;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BlazorApp.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _http;
        private readonly ITokenProvider _tokenProvider;
        private readonly IConfiguration configuration;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // The constructor remains the same. The DI container, thanks to the
        // setup in Program.cs, will inject a pre-configured HttpClient.
        public ApiClient(HttpClient http, ITokenProvider tokenProvider,IConfiguration configuration)
        {
            _http = http;
            _tokenProvider = tokenProvider;
            this.configuration = configuration;
        }


        private string GetBaseUrlIfNotExist(string url)
        {
            // 1. Handle null or empty input URL gracefully.
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            // 2. Check if the provided URL is already absolute.
            // Uri.IsWellFormedUriString is a robust way to check for a scheme (http, https, ftp, etc.).
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return url;
            }
            

            // 3. Use the Uri constructor to safely combine the base and relative paths.
            // This correctly handles all combinations of trailing/leading slashes.
            var baseUri = new Uri(configuration["BaseAddress"]);
            var absoluteUri = new Uri(baseUri, url);

            return absoluteUri.ToString();
        }

        public async Task<ApiResponse<T>> GetAsync<T>(string url, bool authorized = true)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GetBaseUrlIfNotExist(url));
            await AddAuthorizationHeaderAsync(request, authorized);

            var response = await _http.SendAsync(request);
            return await DeserializeApiResponse<T>(response);
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string url, object body, bool authorized = true)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            await AddAuthorizationHeaderAsync(request, authorized);

            var json = JsonSerializer.Serialize(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            return await DeserializeApiResponse<T>(response);
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string url, object body, bool authorized = true)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, url);
            await AddAuthorizationHeaderAsync(request, authorized);

            var json = JsonSerializer.Serialize(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            return await DeserializeApiResponse<T>(response);
        }

        public async Task<ApiResponse<T>> DeleteAsync<T>(string url, bool authorized = true)
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, url);
            await AddAuthorizationHeaderAsync(request, authorized);

            var response = await _http.SendAsync(request);
            return await DeserializeApiResponse<T>(response);
        }

        // *** KEY CHANGE: Add header to the request message, NOT the client's default headers ***
        private async Task AddAuthorizationHeaderAsync(HttpRequestMessage request, bool authorized)
        {
            if (authorized)
            {
                var token = await _tokenProvider.GetToken();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }
            }
        }

        // This method remains the same
        private async Task<ApiResponse<T>> DeserializeApiResponse<T>(HttpResponseMessage response)
        {
            // ... (no changes to this method) ...
            try
            {
                var json = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(json))
                    return new ApiResponse<T>
                    {
                        Data = default,
                        Messages = new() { "Empty response from server" },
                        IsSuccessful = false
                    };

                var result = JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOptions);

                return result ?? new ApiResponse<T>
                {
                    Data = default,
                    Messages = new() { "Unable to deserialize response" },
                    IsSuccessful = false
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Data = default,
                    Messages = new() { ex.Message },
                    IsSuccessful = false
                };
            }
        }
    }
}