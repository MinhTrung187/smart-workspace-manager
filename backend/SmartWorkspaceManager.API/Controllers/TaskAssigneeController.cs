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
    public sealed class TaskAssigneeController : ControllerBase
    {
        private readonly ITaskAssigneeService _service;

        public TaskAssigneeController(ITaskAssigneeService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpPost("assign")]
        public async Task<ActionResult<TaskAssigneeResponse>> Assign([FromBody] AssignTaskRequest request)
        {
            try
            {
                var result = await _service.AssignAsync(request.TaskId, request.UserId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred.", details = ex.Message }); }
        }

        [HttpDelete("{taskId:guid}/assign/{userId:guid}")]
        public async Task<ActionResult> Remove(Guid taskId, Guid userId)
        {
            try
            {
                await _service.RemoveAssignmentAsync(taskId, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred.", details = ex.Message }); }
        }

        [HttpGet("task/{taskId:guid}")]
        public async Task<ActionResult<List<TaskAssigneeResponse>>> GetAssigneesByTask(Guid taskId)
        {
            try
            {
                var list = await _service.GetAssigneesByTaskAsync(taskId);
                return Ok(list);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred.", details = ex.Message }); }
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<List<BoardTaskResponse>>> GetTasksByUser(Guid userId)
        {
            try
            {
                var list = await _service.GetTasksByUserAsync(userId);
                return Ok(list);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred.", details = ex.Message }); }
        }

        [HttpGet("me")]
        public async Task<ActionResult<List<BoardTaskResponse>>> GetMyTasks()
        {
            try
            {
                var list = await _service.GetMyTasksAsync();
                return Ok(list);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred.", details = ex.Message }); }
        }
    }
}
