using edusync_backend.DTOs;
using edusync_backend.Services;
using edusync_backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace edusync_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AssessmentController : ControllerBase
    {
        private readonly IAssessmentService _assessmentService;
        private readonly EduSyncDbContext _context;

        public AssessmentController(IAssessmentService assessmentService, EduSyncDbContext context)
        {
            _assessmentService = assessmentService;
            _context = context;
        }

        // GET: api/assessment/course/{courseId}
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetAssessmentsByCourse(Guid courseId)
        {
            var assessments = await _assessmentService.GetAssessmentsByCourseAsync(courseId);
            return Ok(assessments);
        }

        // GET: api/assessment/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AssessmentDto>> GetAssessment(Guid id)
        {
            var assessment = await _assessmentService.GetAssessmentByIdAsync(id);
            if (assessment == null)
                return NotFound();

            return Ok(assessment);
        }

        // POST: api/assessment
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult> CreateAssessment([FromBody] CreateAssessmentDto dto)
        {
            var assessmentId = await _assessmentService.CreateAssessmentAsync(dto);
            return CreatedAtAction(nameof(GetAssessment), new { id = assessmentId }, new { id = assessmentId });
        }

        // PUT: api/assessment/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult> UpdateAssessment(Guid id, [FromBody] UpdateAssessmentDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Assessment ID mismatch.");

            var result = await _assessmentService.UpdateAssessmentAsync(dto);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // DELETE: api/assessment/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult> DeleteAssessment(Guid id)
        {
            var result = await _assessmentService.DeleteAssessmentAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPost("{assessmentId}/submit")]
        [Authorize(Policy = "RequireStudentRole")]
        public async Task<ActionResult> SubmitAssessment(Guid assessmentId, [FromBody] SubmitAssessmentDto dto)
        {
            var studentId = GetCurrentUserId(); 
            var submissionId = await _assessmentService.SubmitAssessmentAsync(assessmentId, studentId, dto);
            return Ok(new { submissionId });
        }

        [HttpPut("{submissionId}/grade")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult> GradeSubmission(Guid submissionId, [FromBody] GradeSubmissionDto dto)
        {
            var result = await _assessmentService.GradeSubmissionAsync(submissionId, dto);
            if (!result)
                return NotFound();

            return Ok(new { message = "Submission graded successfully" });
        }

        [HttpGet("{assessmentId}/results")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<IEnumerable<AssessmentResultDto>>> GetAssessmentResults(Guid assessmentId)
        {
            var results = await _assessmentService.GetAssessmentResultsAsync(assessmentId);
            return Ok(results);
        }
        [HttpGet("student/{studentId}/results")]
        [Authorize(Roles = "Instructor,Student")]
        public async Task<ActionResult<IEnumerable<AssessmentResultDto>>> GetStudentResults(Guid studentId)
        {
            var results = await _assessmentService.GetStudentAssessmentResultsAsync(studentId);
            return Ok(results);
        }

        [HttpGet("student/{studentId}/upcoming")]
        [Authorize(Roles = "Instructor,Student")]
        public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetUpcomingAssessments(Guid studentId)
        {
            // Get all courses the student is enrolled in
            var enrollments = await _context.Enrollments
                .Where(e => e.UserId == studentId)
                .Select(e => e.CourseId)
                .ToListAsync();

            if (!enrollments.Any())
            {
                return Ok(new List<AssessmentDto>());
            }

            // Get all assessments from enrolled courses that are not yet due
            var upcomingAssessments = await _context.Assessments
                .Include(a => a.Submissions)
                .Where(a => enrollments.Contains(a.CourseId) && a.DueDate > DateTime.UtcNow)
                .OrderBy(a => a.DueDate)
                .Select(a => new AssessmentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    CourseId = a.CourseId,
                    CourseName = a.Course.Name,
                    DueDate = a.DueDate,
                    Status = a.Submissions
                        .Where(s => s.StudentId == studentId)
                        .Select(s => s.Status)
                        .FirstOrDefault() ?? "Not Started",
                    Questions = a.Questions.Select(q => new AssessmentQuestionDto
                    {
                        Id = q.Id,
                        QuestionText = q.QuestionText,
                        QuestionType = q.QuestionType,
                        Options = q.Options,
                        CorrectAnswer = q.CorrectAnswer
                    }).ToList()
                })
                .ToListAsync();

            return Ok(upcomingAssessments);
        }

        [HttpGet("{assessmentId}/result/{studentId}")]
        public async Task<ActionResult<AssessmentResultDto>> GetAssessmentResult(Guid assessmentId, Guid studentId)
        {
            try
            {
                var result = await _assessmentService.GetAssessmentResultAsync(assessmentId, studentId);
                if (result == null)
                {
                    return NotFound($"No assessment result found for assessment {assessmentId} and student {studentId}");
                }
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // Log the full exception details
                Console.WriteLine($"Error in GetAssessmentResult: {ex}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException}");
                }
                return StatusCode(500, $"An error occurred while retrieving the assessment result: {ex.Message}");
            }
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
    }
}
