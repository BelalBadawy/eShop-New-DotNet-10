namespace eShop.Application.Features.Menus.Commands
{
    public class UpdateMenuRequest
    {
        public int Id { get; set; } 
        public string Title { get; set; } = string.Empty;
        public string? Link { get; set; }
        public string Type { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public int Order { get; set; }
    }
}
