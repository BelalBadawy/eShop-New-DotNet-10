using BlazorApp.Interfaces;
using Blazored.LocalStorage;

namespace BlazorApp.Providers
{
    public class LocalStorageTokenProvider : ITokenProvider
    {
        private readonly ILocalStorageService _localStorage;

        public LocalStorageTokenProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<string> GetToken()
        {
            return await _localStorage.GetItemAsync<string>("authToken");
        }
    }
}
