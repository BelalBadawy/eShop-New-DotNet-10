using BlazorApp.Models;

namespace BlazorApp.Interfaces
{
    public interface IApiClient
    {
        Task<ApiResponse<T>> GetAsync<T>(string url, bool authorized = true);
        Task<ApiResponse<T>> PostAsync<T>(string url, object body, bool authorized = true);
        Task<ApiResponse<T>> PutAsync<T>(string url, object body, bool authorized = true);
        Task<ApiResponse<T>> DeleteAsync<T>(string url, bool authorized = true);
    }
}
