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
    public sealed class WorkspaceController : ControllerBase
    {
        private readonly IWorkspaceService _workspaceService;

        public WorkspaceController(IWorkspaceService workspaceService)
        {
            _workspaceService = workspaceService ?? throw new ArgumentNullException(nameof(workspaceService));
        }

        [HttpPost]
        public async Task<ActionResult<WorkspaceResponse>> Create(CreateWorkspaceRequest request)
        {
            try
            {
                var workspace = await _workspaceService.CreateWorkspaceAsync(request);
                return CreatedAtAction(nameof(Create), new { id = workspace.Id }, workspace);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the workspace.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<UserWorkspacesResponse>> GetAll()
        {
            try
            {
                var workspaces = await _workspaceService.GetWorkspacesOfUserAsync();
                return Ok(workspaces);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the workspaces.", details = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<WorkspaceDetailResponse>> GetById(Guid id)
        {
            try
            {
                var workspace = await _workspaceService.GetWorkspaceByIdAsync(id);
                return Ok(workspace);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the workspace details.", details = ex.Message });
            }
        }


    }
}
