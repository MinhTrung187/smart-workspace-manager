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
    [Route("api/chat/channels/{channelId:guid}/messages")]
    public sealed class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        }

        [HttpGet]
        public async Task<ActionResult<List<ChatMessageResponse>>> GetAll(Guid channelId)
        {
            try
            {
                var messages = await _chatService.GetMessagesAsync(channelId);
                return Ok(messages);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving messages.", details = ex.Message }); }
        }

        [HttpPost]
        public async Task<ActionResult<ChatMessageResponse>> Send(Guid channelId, [FromBody] CreateChatMessageRequest request)
        {
            try
            {
                var message = await _chatService.SendMessageAsync(channelId, request);
                return CreatedAtAction(nameof(GetAll), new { channelId = channelId }, message);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while sending message.", details = ex.Message }); }
        }
    }
}
