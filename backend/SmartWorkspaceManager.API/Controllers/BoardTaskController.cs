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
    public sealed class BoardTaskController : ControllerBase
    {
        private readonly IBoardTaskService _taskService;

        public BoardTaskController(IBoardTaskService taskService)
        {
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        }

        [HttpPost]
        public async Task<ActionResult<BoardTaskResponse>> Create(CreateBoardTaskRequest request)
        {
            try
            {
                var task = await _taskService.CreateTaskAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the task.", details = ex.Message });
            }
        }

        [HttpGet("column/{columnId:guid}")]
        public async Task<ActionResult<List<BoardTaskResponse>>> GetByColumn(Guid columnId)
        {
            try
            {
                var tasks = await _taskService.GetTasksByColumnAsync(columnId);
                return Ok(tasks);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving tasks.", details = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BoardTaskResponse>> GetById(Guid id)
        {
            try
            {
                var task = await _taskService.GetTaskByIdAsync(id);
                return Ok(task);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the task.", details = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<BoardTaskResponse>> Update(Guid id, UpdateBoardTaskRequest request)
        {
            try
            {
                var task = await _taskService.UpdateTaskAsync(id, request);
                return Ok(task);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the task.", details = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _taskService.DeleteTaskAsync(id);
                return NoContent();
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the task.", details = ex.Message });
            }
        }
    }
}
