namespace eShop.Application.Features.Roles.Commands
{
    public class UpdateRoleRequest
    {
        public int RoleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
