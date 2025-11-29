using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Models.Menus
{
    public class MenuDto
    {
        // Assuming Id is handled by the backend/API
        // public int Id { get; set; }

        [Required(ErrorMessage = "Menu title is required.")]
        [StringLength(100, ErrorMessage = "Menu title cannot exceed 100 characters.")]
        public string MenuTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Link is required.")]
        [StringLength(255, ErrorMessage = "Link cannot exceed 255 characters.")]
        public string Link { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a menu type.")]
        public string Type { get; set; } = string.Empty;

        // Assuming ParentId is nullable. A null value means "No Parent".
        // If you wanted to force a parent, you would add [Required].
        public int? ParentId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Order must be a non-negative number.")]
        public int Order { get; set; }
    }
}
