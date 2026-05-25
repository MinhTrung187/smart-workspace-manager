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
    public sealed class ColumnController : ControllerBase
    {
        private readonly IColumnService _columnService;

        public ColumnController(IColumnService columnService)
        {
            _columnService = columnService ?? throw new ArgumentNullException(nameof(columnService));
        }

        [HttpPost]
        public async Task<ActionResult<ColumnResponse>> Create(CreateColumnRequest request)
        {
            try
            {
                var column = await _columnService.CreateColumnAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = column.Id }, column);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the column.", details = ex.Message });
            }
        }

        [HttpGet("board/{boardId:guid}")]
        public async Task<ActionResult<List<ColumnResponse>>> GetByBoard(Guid boardId)
        {
            try
            {
                var columns = await _columnService.GetColumnsByBoardAsync(boardId);
                return Ok(columns);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving columns.", details = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ColumnResponse>> GetById(Guid id)
        {
            try
            {
                var column = await _columnService.GetColumnByIdAsync(id);
                return Ok(column);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the column.", details = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ColumnResponse>> Update(Guid id, UpdateColumnRequest request)
        {
            try
            {
                var column = await _columnService.UpdateColumnAsync(id, request);
                return Ok(column);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the column.", details = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _columnService.DeleteColumnAsync(id);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the column.", details = ex.Message });
            }
        }
    }
}
