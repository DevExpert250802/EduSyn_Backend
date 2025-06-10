using edusync_backend.Data;
using edusync_backend.DTOs;
using edusync_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace edusync_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CourseController : ControllerBase
    {
        private readonly EduSyncDbContext _context;

        public CourseController(EduSyncDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDTO>>> GetCourses()
        {
            var courses = await _context.Courses.ToListAsync();

            var courseDTOs = courses.Select(course => new CourseDTO
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId,
                MediaUrl = course.MediaUrl
            });

            return Ok(courseDTOs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDTO>> GetCourse(Guid id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            var dto = new CourseDTO
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId,
                MediaUrl = course.MediaUrl
            };

            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<CourseDTO>> CreateCourse(CreateCourseDTO createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.Title)) return BadRequest("Course title is required.");
            if (string.IsNullOrWhiteSpace(createDto.Description)) return BadRequest("Course description is required.");
            if (createDto.Title.Length > 100) return BadRequest("Course title must be less than 100 characters.");
            if (createDto.Description.Length > 1000) return BadRequest("Course description must be less than 1000 characters.");

            var course = new Course
            {
                CourseId = Guid.NewGuid(),
                Name = createDto.Title,
                Title = createDto.Title,
                Description = createDto.Description,
                MediaUrl = createDto.MediaUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (User.IsInRole("Instructor"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    throw new UnauthorizedAccessException("User ID not found in claims");
                }
                course.InstructorId = Guid.Parse(userIdClaim);
            }
            else
            {
                course.InstructorId = createDto.InstructorId;
            }

            _context.Courses.Add(course);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the inner exception details
                var innerException = ex.InnerException?.Message ?? "No inner exception";
                return StatusCode(500, new { error = "An error occurred while saving the course.", details = innerException });
            }

            var courseDto = new CourseDTO
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId,
                MediaUrl = course.MediaUrl
            };

            return CreatedAtAction(nameof(GetCourse), new { id = course.CourseId }, courseDto);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> UpdateCourse(Guid id, UpdateCourseDTO updateDto)
        {
            if (id != updateDto.CourseId) return BadRequest("Course ID mismatch.");
            if (string.IsNullOrWhiteSpace(updateDto.Title)) return BadRequest("Course title is required.");
            if (string.IsNullOrWhiteSpace(updateDto.Description)) return BadRequest("Course description is required.");
            if (updateDto.Title.Length > 100) return BadRequest("Course title must be less than 100 characters.");
            if (updateDto.Description.Length > 1000) return BadRequest("Course description must be less than 1000 characters.");

            var existingCourse = await _context.Courses.FindAsync(id);
            if (existingCourse == null) return NotFound();

            if (User.IsInRole("Instructor"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    throw new UnauthorizedAccessException("User ID not found in claims");
                }
                var instructorId = Guid.Parse(userIdClaim);
                if (existingCourse.InstructorId != instructorId) return Forbid();
            }

            existingCourse.Title = updateDto.Title;
            existingCourse.Description = updateDto.Description;
            existingCourse.MediaUrl = updateDto.MediaUrl;
            existingCourse.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                .Include(c => c.Assessments)
                    .ThenInclude(a => a.Questions)
                .Include(c => c.Assessments)
                    .ThenInclude(a => a.Submissions)
                        .ThenInclude(s => s.Answers)
                .Include(c => c.Announcements)
                .Include(c => c.ForumPosts)
                    .ThenInclude(p => p.Replies)
                .Include(c => c.CalendarEvents)
                // .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null) return NotFound();

            if (User.IsInRole("Instructor"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    throw new UnauthorizedAccessException("User ID not found in claims");
                }
                var instructorId = Guid.Parse(userIdClaim);
                if (course.InstructorId != instructorId) return Forbid();
            }

            try
            {
                foreach (var assessment in course.Assessments)
                {
                    foreach (var submission in assessment.Submissions)
                    {
                        _context.AssessmentAnswers.RemoveRange(submission.Answers);
                    }
                    _context.AssessmentSubmissions.RemoveRange(assessment.Submissions);
                    _context.AssessmentQuestions.RemoveRange(assessment.Questions);
                }
                _context.Assessments.RemoveRange(course.Assessments);

                _context.Enrollments.RemoveRange(course.Enrollments);
                _context.Announcements.RemoveRange(course.Announcements);
                
                foreach (var post in course.ForumPosts)
                {
                    _context.ForumPosts.RemoveRange(post.Replies);
                }
                _context.ForumPosts.RemoveRange(course.ForumPosts);
                
                _context.CalendarEvents.RemoveRange(course.CalendarEvents);
                // _context.Messages.RemoveRange(course.Messages);
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "An error occurred while deleting the course.",
                    detailedMessage = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpGet("instructor")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<IEnumerable<CourseDTO>>> GetInstructorCourses()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return BadRequest("User ID not found in claims");
            }

            var instructorId = Guid.Parse(userIdClaim);
            var courses = await _context.Courses
                .Where(c => c.InstructorId == instructorId)
                .ToListAsync();

            var courseDTOs = courses.Select(course => new CourseDTO
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId,
                MediaUrl = course.MediaUrl
            });

            return Ok(courseDTOs);
        }
        

        [HttpGet("{id}/students")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<IEnumerable<StudentDTO>>> GetCourseStudents(Guid id)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            // Verify the instructor owns this course
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            var instructorId = Guid.Parse(userIdClaim);
            if (course.InstructorId != instructorId)
            {
                return Forbid();
            }

            var students = course.Enrollments
                .Select(e => new StudentDTO
                {
                    Id = e.User.UserId,
                    Name = e.User.Name,
                    Email = e.User.Email,
                    EnrollmentDate = e.EnrollmentDate,
                    Progress = e.Progress,
                    Status = e.Status
                })
                .ToList();

            return Ok(students);
        }
    }
}
