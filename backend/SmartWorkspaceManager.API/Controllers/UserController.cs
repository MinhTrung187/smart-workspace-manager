using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartWorkspaceManager.Application.Interfaces;

namespace SmartWorkspaceManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet("GetAllUser")]
        public async Task<IActionResult> GetAllUser()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
    }
}