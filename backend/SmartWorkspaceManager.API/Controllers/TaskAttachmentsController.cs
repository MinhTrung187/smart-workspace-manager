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
    [Route("api/[controller]")]
    public sealed class TaskAttachmentsController : ControllerBase
    {
        private readonly ITaskAttachmentService _attachmentService;

        public TaskAttachmentsController(ITaskAttachmentService attachmentService)
        {
            _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
        }

        [HttpPost("{taskId:guid}")]
        public async Task<IActionResult> Upload(Guid taskId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File is required." });

            try
            {
                await using var stream = file.OpenReadStream();
                var dto = await _attachmentService.AddAttachmentAsync(taskId, stream, file.FileName, file.ContentType);
                return CreatedAtAction(nameof(GetByTask), new { taskId = taskId }, dto);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message }); }
        }

        [HttpGet("task/{taskId:guid}")]
        public async Task<IActionResult> GetByTask(Guid taskId)
        {
            try
            {
                var list = await _attachmentService.GetAttachmentsByTaskAsync(taskId);
                return Ok(list);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message }); }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _attachmentService.DeleteAttachmentAsync(id);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message }); }
        }
    }
}
