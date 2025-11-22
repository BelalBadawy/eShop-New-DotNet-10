namespace BlazorApp.Interfaces
{
    public interface ITokenProvider
    {
        Task<string> GetToken();
    }
}
