namespace eShop.Application.Features.Menus.Commands
{
    public class CreateMenuRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Link { get; set; }
        public string Type { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public int Order { get; set; }
    }
}
