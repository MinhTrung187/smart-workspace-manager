using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartWorkspaceManager.Application.Interfaces;

namespace SmartWorkspaceManager.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/workspaces/{workspaceId:guid}/activitylogs")]
    public sealed class ActivityLogsController : ControllerBase
    {
        private readonly IActivityLogService _activityLogService;

        public ActivityLogsController(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
        }

        [HttpGet] 
        public async Task<IActionResult> GetAll(Guid workspaceId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var result = await _activityLogService.GetWorkspaceLogsAsync(workspaceId, page, pageSize);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving activity logs.", details = ex.Message });
            }
        }
    }
}
