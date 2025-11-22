namespace BlazorApp.Models
{
    public class ApiResponse<T>
    {
        public T Data { get; set; }
        public List<string> Messages { get; set; }
        public bool IsSuccessful { get; set; }
    }
}
