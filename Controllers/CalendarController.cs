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
    public class CalendarController : ControllerBase
    {
        private readonly EduSyncDbContext _context;

        public CalendarController(EduSyncDbContext context)
        {
            _context = context;
        }

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<CalendarEventDTO>>> GetCourseEvents(Guid courseId)
        {
            var events = await _context.CalendarEvents
                .Where(e => e.CourseId == courseId)
                .OrderBy(e => e.StartTime)
                .Select(e => new CalendarEventDTO
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    EventType = e.EventType,
                    Location = e.Location,
                    CourseId = e.CourseId,
                    CourseName = e.Course.Name,
                    CreatedById = e.CreatedById,
                    CreatedByName = e.CreatedBy.Name,
                    IsRecurring = e.IsRecurring,
                    RecurrencePattern = e.RecurrencePattern,
                    RecurrenceEndDate = e.RecurrenceEndDate,
                    ReminderSettings = e.ReminderSettings
                })
                .ToListAsync();

            return Ok(events);
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<CalendarEventDTO>>> GetUserEvents()
        {
            var userIdClaim = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }
            var userId = Guid.Parse(userIdClaim);
            var userEnrollments = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();

            var events = await _context.CalendarEvents
                .Where(e => userEnrollments.Contains(e.CourseId))
                .OrderBy(e => e.StartTime)
                .Select(e => new CalendarEventDTO
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    EventType = e.EventType,
                    Location = e.Location,
                    CourseId = e.CourseId,
                    CourseName = e.Course.Name,
                    CreatedById = e.CreatedById,
                    CreatedByName = e.CreatedBy.Name,
                    IsRecurring = e.IsRecurring,
                    RecurrencePattern = e.RecurrencePattern,
                    RecurrenceEndDate = e.RecurrenceEndDate,
                    ReminderSettings = e.ReminderSettings
                })
                .ToListAsync();

            return Ok(events);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<CalendarEventResponseDTO>> CreateEvent(CreateCalendarEventDTO eventDto)
        {
            var createdByIdClaim = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(createdByIdClaim))
            {
                return Unauthorized();
            }
            var createdById = Guid.Parse(createdByIdClaim);
            var calendarEvent = new CalendarEvent
            {
                Id = Guid.NewGuid(),
                Title = eventDto.Title,
                Description = eventDto.Description,
                StartTime = eventDto.StartTime,
                EndTime = eventDto.EndTime,
                EventType = eventDto.EventType,
                Location = eventDto.Location,
                CourseId = eventDto.CourseId,
                CreatedById = createdById,
                IsRecurring = eventDto.IsRecurring,
                RecurrencePattern = eventDto.RecurrencePattern,
                RecurrenceEndDate = eventDto.RecurrenceEndDate,
                ReminderSettings = eventDto.ReminderSettings
            };

            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();

            var response = new CalendarEventResponseDTO
            {
                Success = true,
                Message = "Event created successfully",
                Data = new CalendarEventDTO
                {
                    Id = calendarEvent.Id,
                    Title = calendarEvent.Title,
                    Description = calendarEvent.Description,
                    StartTime = calendarEvent.StartTime,
                    EndTime = calendarEvent.EndTime,
                    EventType = calendarEvent.EventType,
                    Location = calendarEvent.Location,
                    CourseId = calendarEvent.CourseId,
                    CourseName = (await _context.Courses.FindAsync(calendarEvent.CourseId))?.Name ?? "Unknown Course",
                    CreatedById = calendarEvent.CreatedById,
                    CreatedByName = (await _context.Users.FindAsync(calendarEvent.CreatedById))?.Name ?? "Unknown User",
                    IsRecurring = calendarEvent.IsRecurring,
                    RecurrencePattern = calendarEvent.RecurrencePattern,
                    RecurrenceEndDate = calendarEvent.RecurrenceEndDate,
                    ReminderSettings = calendarEvent.ReminderSettings
                }
            };

            return Ok(response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<CalendarEventResponseDTO>> UpdateEvent(Guid id, CreateCalendarEventDTO eventDto)
        {
            var calendarEvent = await _context.CalendarEvents.FindAsync(id);
            if (calendarEvent == null)
                return NotFound();

            var userIdClaim = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }
            var userId = Guid.Parse(userIdClaim);
            if (calendarEvent.CreatedById != userId)
                return Forbid();

            calendarEvent.Title = eventDto.Title;
            calendarEvent.Description = eventDto.Description;
            calendarEvent.StartTime = eventDto.StartTime;
            calendarEvent.EndTime = eventDto.EndTime;
            calendarEvent.EventType = eventDto.EventType;
            calendarEvent.Location = eventDto.Location;
            calendarEvent.IsRecurring = eventDto.IsRecurring;
            calendarEvent.RecurrencePattern = eventDto.RecurrencePattern;
            calendarEvent.RecurrenceEndDate = eventDto.RecurrenceEndDate;
            calendarEvent.ReminderSettings = eventDto.ReminderSettings;

            await _context.SaveChangesAsync();

            var response = new CalendarEventResponseDTO
            {
                Success = true,
                Message = "Event updated successfully",
                Data = new CalendarEventDTO
                {
                    Id = calendarEvent.Id,
                    Title = calendarEvent.Title,
                    Description = calendarEvent.Description,
                    StartTime = calendarEvent.StartTime,
                    EndTime = calendarEvent.EndTime,
                    EventType = calendarEvent.EventType,
                    Location = calendarEvent.Location,
                    CourseId = calendarEvent.CourseId,
                    CourseName = (await _context.Courses.FindAsync(calendarEvent.CourseId))?.Name ?? "Unknown Course",
                    CreatedById = calendarEvent.CreatedById,
                    CreatedByName = (await _context.Users.FindAsync(calendarEvent.CreatedById))?.Name ?? "Unknown User",
                    IsRecurring = calendarEvent.IsRecurring,
                    RecurrencePattern = calendarEvent.RecurrencePattern,
                    RecurrenceEndDate = calendarEvent.RecurrenceEndDate,
                    ReminderSettings = calendarEvent.ReminderSettings
                }
            };

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult> DeleteEvent(Guid id)
        {
            var calendarEvent = await _context.CalendarEvents.FindAsync(id);
            if (calendarEvent == null)
                return NotFound();

            var userIdClaim = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }
            var userId = Guid.Parse(userIdClaim);
            if (calendarEvent.CreatedById != userId)
                return Forbid();

            _context.CalendarEvents.Remove(calendarEvent);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Event deleted successfully" });
        }
    }
} 