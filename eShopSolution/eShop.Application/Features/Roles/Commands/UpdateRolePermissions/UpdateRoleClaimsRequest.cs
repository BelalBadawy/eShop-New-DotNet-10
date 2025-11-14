namespace eShop.Application.Features.Roles.Commands
{
    public class UpdateRoleClaimsRequest
    {
        public int RoleId { get; set; }
        public List<RoleClaimViewModel> RoleClaims { get; set; }
    }
}
