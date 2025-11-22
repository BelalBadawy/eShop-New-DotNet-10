using eShop.Application.Features.Menus.Commands;
using eShop.Application.Features.Menus.Queries;

namespace eShop.API.Controllers
{
    public class MenusController : BaseApiController
    {

        [HttpGet()]
        [AllowAnonymous]
        public async Task<IActionResult> GetMenusAsync([FromQuery] string type)
        {
            var response = await Sender.Send(new GetAllByTypeQuery() { Type = type });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpGet("for-list")]
        [MustHavePermission(AppService.Website, AppFeature.Menus, AppAction.Read)]
        public async Task<IActionResult> GetMenusForListAsync(string type)
        {
            var response = await Sender.Send(new GetAllMenusForListByTypeQuery() { Type = type });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpGet("{menuId:int}")]
        [MustHavePermission(AppService.Website, AppFeature.Menus, AppAction.Read)]
        public async Task<IActionResult> GetMenuByIdAsync(int menuId)
        {
            var response = await Sender.Send(new GetMenuByIdQuery() { MenuId = menuId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost()]
        [MustHavePermission(AppService.Website, AppFeature.Menus, AppAction.Create)]
        public async Task<IActionResult> CreateMenusAsync([FromBody] CreateMenuRequest request)
        {
            var response = await Sender.Send(new CreateMenuCommand() { CreateMenuRequest = request });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPut()]
        [MustHavePermission(AppService.Website, AppFeature.Menus, AppAction.Update)]
        public async Task<IActionResult> UpdateMenusAsync([FromBody] UpdateMenuRequest request)
        {
            var response = await Sender.Send(new UpdateMenuCommand() { UpdateMenuRequest = request });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }


        [HttpDelete("{menuId:int}")]
        [MustHavePermission(AppService.Website, AppFeature.Menus, AppAction.Delete)]
        public async Task<IActionResult> DeleteMenu(int menuId)
        {
            var response = await Sender.Send(new DeleteMenuCommand { MenuId = menuId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

    }
}
