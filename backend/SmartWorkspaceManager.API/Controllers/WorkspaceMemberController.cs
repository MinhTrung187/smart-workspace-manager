using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartWorkspaceManager.Application.DTOs;
using SmartWorkspaceManager.Application.Interfaces;

namespace SmartWorkspaceManager.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/workspaces/{workspaceId:guid}/members")]
    public sealed class WorkspaceMemberController : ControllerBase
    {
        private readonly IWorkspaceMemberService _memberService;

        public WorkspaceMemberController(IWorkspaceMemberService memberService)
        {
            _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
        }

        [HttpGet]
        public async Task<ActionResult<List<WorkspaceMemberDto>>> GetAll(Guid workspaceId)
        {
            try
            {
                var members = await _memberService.GetMembersByWorkspaceAsync(workspaceId);
                return Ok(members);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving workspace members.", details = ex.Message });
            }
        }
        [HttpDelete("{userId:guid}")]
        public async Task<IActionResult> Remove(Guid workspaceId, Guid userId)
        {
            try
            {
                await _memberService.RemoveMemberAsync(workspaceId, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while removing the workspace member.", details = ex.Message });
            }
        }
    }
}
