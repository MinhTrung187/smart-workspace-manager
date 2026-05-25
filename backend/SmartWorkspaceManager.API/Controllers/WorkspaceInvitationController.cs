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
    [Route("api/[controller]")]
    public sealed class WorkspaceInvitationController : ControllerBase
    {
        private readonly IWorkspaceInvitationService _invitationService;

        public WorkspaceInvitationController(IWorkspaceInvitationService invitationService)
        {
            _invitationService = invitationService ?? throw new ArgumentNullException(nameof(invitationService));
        }

        [HttpGet]
        public async Task<ActionResult<List<AllInvitationResponse>>> GetAll()
        {
            try
            {
                var invites = await _invitationService.GetInvitationsForCurrentUserAsync();
                return Ok(invites);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving invitations.", details = ex.Message });
            }
        }
        [HttpPost("{id:guid}/accept")]
        public async Task<ActionResult<WorkspaceMemberDto>> Accept(Guid id)
        {
            try
            {
                var member = await _invitationService.AcceptInvitationAsync(id);
                return Ok(member);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while accepting the invitation.", details = ex.Message });
            }
        }
    }
}
