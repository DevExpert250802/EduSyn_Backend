using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using edusync_backend.Data;
using edusync_backend.Models;
using edusync_backend.DTOs;
using System.Security.Claims;

namespace edusync_backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly EduSyncDbContext _context;

        public MessageController(EduSyncDbContext context)
        {
            _context = context;
        }

        [HttpGet("conversations")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetConversations()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var messages = await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderByDescending(m => m.Timestamp)
                .Select(m => new MessageDTO
                {
                    Id = m.Id,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    IsRead = m.IsRead,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.Name,
                    ReceiverId = m.ReceiverId,
                    ReceiverName = m.Receiver.Name,
                    CourseId = m.CourseId,
                    CourseName = m.Course.Name
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpGet("conversation/{otherUserId}")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetConversation(string otherUserId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var otherUserGuid = Guid.Parse(otherUserId);

            var messages = await _context.Messages
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserGuid) ||
                           (m.SenderId == otherUserGuid && m.ReceiverId == userId))
                .OrderBy(m => m.Timestamp)
                .Select(m => new MessageDTO
                {
                    Id = m.Id,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    IsRead = m.IsRead,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.Name,
                    ReceiverId = m.ReceiverId,
                    ReceiverName = m.Receiver.Name,
                    CourseId = m.CourseId,
                    CourseName = m.Course.Name
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost]
        public async Task<ActionResult<MessageDTO>> SendMessage(CreateMessageDTO messageDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var message = new Message
            {
                Content = messageDto.Content,
                Timestamp = DateTime.UtcNow,
                IsRead = false,
                SenderId = userId,
                ReceiverId = messageDto.ReceiverId,
                CourseId = messageDto.CourseId
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var sender = await _context.Users.FindAsync(userId);
            var receiver = await _context.Users.FindAsync(messageDto.ReceiverId);
            var course = await _context.Courses.FindAsync(messageDto.CourseId);

            if (sender == null || receiver == null || course == null)
            {
                return NotFound("User or course not found");
            }

            var responseDto = new MessageDTO
            {
                Id = message.Id,
                Content = message.Content,
                Timestamp = message.Timestamp,
                IsRead = message.IsRead,
                SenderId = message.SenderId,
                SenderName = sender.Name,
                ReceiverId = message.ReceiverId,
                ReceiverName = receiver.Name,
                CourseId = message.CourseId,
                CourseName = course.Name
            };

            return Ok(responseDto);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var messageId = Guid.Parse(id);

            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
            {
                return NotFound();
            }

            if (message.ReceiverId != userId)
            {
                return Forbid();
            }

            message.IsRead = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(string id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var messageId = Guid.Parse(id);

            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
            {
                return NotFound();
            }

            if (message.SenderId != userId)
            {
                return Forbid();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
} 