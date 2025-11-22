namespace eShop.Application.Features.Menus.Queries
{
    public class MenuResponse
    {
        public int Id { get; set; } 
        public string Title { get; set; } = string.Empty;
        public string? Link { get; set; }
        public string Type { get; set; } 
        public int? ParentId { get; set; }
        public int Order { get; set; }
    }
}
