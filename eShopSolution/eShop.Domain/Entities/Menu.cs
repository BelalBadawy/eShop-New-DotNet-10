namespace eShop.Domain.Entities
{
    public class Menu : BaseEntity<int> 
    {
        [StringLength(50)]
        public string Title { get; set; }= string.Empty;

        [StringLength(300)]
        public string? Link { get; set; }

        [StringLength(20)]
        public string Type { get; set; } = string.Empty;

        public int? ParentId { get; set; }
        public int Order { get; set; }
    }
}
