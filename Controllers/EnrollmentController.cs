using edusync_backend.Data;
using edusync_backend.Models;
using edusync_backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace edusync_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrollmentController : ControllerBase
    {
        private readonly EduSyncDbContext _context;

        public EnrollmentController(EduSyncDbContext context)
        {
            _context = context;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User ID not found in claims");
            }
            return Guid.Parse(userIdClaim.Value);
        }

        [HttpPost("enroll")]
        [Authorize(Policy = "RequireStudentRole")]
        public async Task<ActionResult<EnrollmentDTO>> EnrollInCourse([FromBody] CreateEnrollmentDTO dto)
        {
            var userId = GetCurrentUserId();

            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == dto.CourseId && e.UserId == userId);

            if (existingEnrollment != null)
            {
                return BadRequest("You are already enrolled in this course.");
            }

            var enrollment = new Enrollment
            {
                EnrollmentId = Guid.NewGuid(),
                CourseId = dto.CourseId,
                UserId = userId,
                EnrollmentDate = DateTime.UtcNow,
                Progress = 0,
                Status = "Active"
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            var responseDto = new EnrollmentDTO
            {
                EnrollmentId = enrollment.EnrollmentId,
                CourseId = enrollment.CourseId,
                UserId = enrollment.UserId,
                EnrollmentDate = enrollment.EnrollmentDate,
                Progress = enrollment.Progress,
                Status = enrollment.Status,
                CompletionDate = enrollment.CompletionDate
            };

            return CreatedAtAction(nameof(GetEnrollment), new { id = enrollment.EnrollmentId }, responseDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EnrollmentDTO>> GetEnrollment(Guid id)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            if (enrollment == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Student"))
            {
                var userId = GetCurrentUserId();
                if (enrollment.UserId != userId)
                {
                    return Forbid();
                }
            }

            var dto = new EnrollmentDTO
            {
                EnrollmentId = enrollment.EnrollmentId,
                CourseId = enrollment.CourseId,
                UserId = enrollment.UserId,
                EnrollmentDate = enrollment.EnrollmentDate,
                Progress = enrollment.Progress,
                Status = enrollment.Status,
                CompletionDate = enrollment.CompletionDate
            };

            return dto;
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<EnrollmentDTO>>> GetStudentEnrollments(Guid studentId)
        {
            if (User.IsInRole("Student"))
            {
                var userId = GetCurrentUserId();
                if (studentId != userId)
                {
                    return Forbid();
                }
            }

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                .Where(e => e.UserId == studentId)
                .ToListAsync();

            var dtos = enrollments.Select(e => new EnrollmentDTO
            {
                EnrollmentId = e.EnrollmentId,
                CourseId = e.CourseId,
                UserId = e.UserId,
                EnrollmentDate = e.EnrollmentDate,
                Progress = e.Progress,
                Status = e.Status,
                CompletionDate = e.CompletionDate,
                Course = new CourseDTO
                {
                    CourseId = e.Course.CourseId,
                    Title = e.Course.Title ?? e.Course.Name,
                    Description = e.Course.Description,
                    InstructorId = e.Course.InstructorId,
                    MediaUrl = e.Course.MediaUrl,
                }
            });

            return Ok(dtos);
        }

        [HttpPut("{id}/progress")]
        [Authorize(Policy = "RequireStudentRole")]
        public async Task<IActionResult> UpdateProgress(Guid id, [FromBody] UpdateProgressDTO dto)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            if (enrollment.UserId != userId)
            {
                return Forbid();
            }

            enrollment.Progress = dto.Progress;
            if (dto.Progress >= 100)
            {
                enrollment.Status = "Completed";
                enrollment.CompletionDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("course/{courseId}/students")]
        [Authorize(Policy = "RequireInstructorRole")]
        public async Task<ActionResult<IEnumerable<StudentDTO>>> GetEnrolledStudents(Guid courseId)
        {
            var userId = GetCurrentUserId();

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
            {
                return NotFound("Course not found.");
            }

            if (course.InstructorId != userId)
            {
                return Forbid("You are not the instructor of this course.");
            }

            var students = await _context.Enrollments
                .Where(e => e.CourseId == courseId)
                .Include(e => e.User)
                .Select(e => new StudentDTO
                {
                    Id = e.User.UserId,
                    Name = e.User.Name,
                    Email = e.User.Email
                })
                .ToListAsync();

            return Ok(students);
        }
    }
}
