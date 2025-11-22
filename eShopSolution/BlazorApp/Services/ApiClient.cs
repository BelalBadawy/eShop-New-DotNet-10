using BlazorApp.Interfaces;
using BlazorApp.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BlazorApp.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _http;
        private readonly ITokenProvider _tokenProvider;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiClient(HttpClient http, ITokenProvider tokenProvider)
        {
            _http = http;
            _tokenProvider = tokenProvider;
        }

        private async Task AddAuthorizationHeaderAsync(bool authorized)
        {
            _http.DefaultRequestHeaders.Authorization = null;

            if (authorized)
            {
                var token = await _tokenProvider.GetToken();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _http.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }
            }
        }

        public async Task<ApiResponse<T>> GetAsync<T>(string url, bool authorized = true)
        {
            await AddAuthorizationHeaderAsync(authorized);

            var response = await _http.GetAsync(url);

            return await DeserializeApiResponse<T>(response);
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string url, object body, bool authorized = true)
        {
            await AddAuthorizationHeaderAsync(authorized);

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(url, content);

            return await DeserializeApiResponse<T>(response);
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string url, object body, bool authorized = true)
        {
            await AddAuthorizationHeaderAsync(authorized);

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PutAsync(url, content);

            return await DeserializeApiResponse<T>(response);
        }

        public async Task<ApiResponse<T>> DeleteAsync<T>(string url, bool authorized = true)
        {
            await AddAuthorizationHeaderAsync(authorized);

            var response = await _http.DeleteAsync(url);

            return await DeserializeApiResponse<T>(response);
        }


        private async Task<ApiResponse<T>> DeserializeApiResponse<T>(HttpResponseMessage response)
        {
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
