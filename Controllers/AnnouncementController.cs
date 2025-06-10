using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using edusync_backend.Data;
using edusync_backend.DTOs;
using edusync_backend.Models;

namespace edusync_backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementController : ControllerBase
    {
        private readonly EduSyncDbContext _context;

        public AnnouncementController(EduSyncDbContext context)
        {
            _context = context;
        }

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<AnnouncementDTO>>> GetCourseAnnouncements(Guid courseId)
        {
            var announcements = await _context.Announcements
                .Where(a => a.CourseId == courseId && (!a.ExpiryDate.HasValue || a.ExpiryDate > DateTime.UtcNow))
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AnnouncementDTO
                {
                    Id = a.Id,
                    Title = a.Title,
                    Content = a.Content,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    CourseId = a.CourseId,
                    CourseName = a.Course.Name,
                    CreatedById = a.CreatedById,
                    CreatedByName = a.CreatedBy.Name,
                    IsPinned = a.IsPinned,
                    ExpiryDate = a.ExpiryDate
                })
                .ToListAsync();

            return Ok(announcements);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<AnnouncementResponseDTO>> CreateAnnouncement(CreateAnnouncementDTO announcementDto)
        {
            var announcement = new Announcement
            {
                Id = Guid.NewGuid(),
                Title = announcementDto.Title,
                Content = announcementDto.Content,
                CreatedAt = DateTime.UtcNow,
                CourseId = announcementDto.CourseId,
                CreatedById = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException()),
                IsPinned = announcementDto.IsPinned,
                ExpiryDate = announcementDto.ExpiryDate
            };

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            var response = new AnnouncementResponseDTO
            {
                Success = true,
                Message = "Announcement created successfully",
                Data = new AnnouncementDTO
                {
                    Id = announcement.Id,
                    Title = announcement.Title,
                    Content = announcement.Content,
                    CreatedAt = announcement.CreatedAt,
                    UpdatedAt = announcement.UpdatedAt,
                    CourseId = announcement.CourseId,
                    CourseName = (await _context.Courses.FindAsync(announcement.CourseId))?.Name ?? "Unknown Course",
                    CreatedById = announcement.CreatedById,
                    CreatedByName = (await _context.Users.FindAsync(announcement.CreatedById))?.Name ?? "Unknown User",
                    IsPinned = announcement.IsPinned,
                    ExpiryDate = announcement.ExpiryDate
                }
            };

            return Ok(response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<AnnouncementResponseDTO>> UpdateAnnouncement(Guid id, CreateAnnouncementDTO announcementDto)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
                return NotFound();

            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());
            if (announcement.CreatedById != userId)
                return Forbid();

            announcement.Title = announcementDto.Title;
            announcement.Content = announcementDto.Content;
            announcement.UpdatedAt = DateTime.UtcNow;
            announcement.IsPinned = announcementDto.IsPinned;
            announcement.ExpiryDate = announcementDto.ExpiryDate;

            await _context.SaveChangesAsync();

            var response = new AnnouncementResponseDTO
            {
                Success = true,
                Message = "Announcement updated successfully",
                Data = new AnnouncementDTO
                {
                    Id = announcement.Id,
                    Title = announcement.Title,
                    Content = announcement.Content,
                    CreatedAt = announcement.CreatedAt,
                    UpdatedAt = announcement.UpdatedAt,
                    CourseId = announcement.CourseId,
                    CourseName = (await _context.Courses.FindAsync(announcement.CourseId))?.Name ?? "Unknown Course",
                    CreatedById = announcement.CreatedById,
                    CreatedByName = (await _context.Users.FindAsync(announcement.CreatedById))?.Name ?? "Unknown User",
                    IsPinned = announcement.IsPinned,
                    ExpiryDate = announcement.ExpiryDate
                }
            };

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult> DeleteAnnouncement(Guid id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
                return NotFound();

            var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException();
            var userId = Guid.Parse(userIdClaim);

            // Allow the creator, any Instructor, or any Admin to delete
            if (announcement.CreatedById != userId && !User.IsInRole("Instructor") && !User.IsInRole("Admin"))
                return Forbid();

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Announcement deleted successfully" });
        }
    }
} 