using edusync_backend.Data;
using edusync_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using edusync_backend.DTOs;

namespace edusync_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ResultController : ControllerBase
    {
        private readonly EduSyncDbContext _context;

        public ResultController(EduSyncDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<IEnumerable<AssessmentResult>>> GetResults()
        {
            var results = await _context.Results
                .Include(r => r.Assessment)
                .Include(r => r.Student)
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Policy = "RequireAdminOrInstructorOrStudentRole")]
        public async Task<ActionResult<IEnumerable<AssessmentResult>>> GetStudentResults(string studentId)
        {
            var userId = Guid.Parse(studentId);
            var results = await _context.Results
                .Include(r => r.Assessment)
                .Where(r => r.StudentId == userId)
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet("assessment/{assessmentId}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<IEnumerable<AssessmentResult>>> GetAssessmentResults(string assessmentId)
        {
            var id = Guid.Parse(assessmentId);
            var results = await _context.Results
                .Include(r => r.Student)
                .Where(r => r.AssessmentId == id)
                .ToListAsync();

            return Ok(results);
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<AssessmentResult>> CreateResult(AssessmentResult result)
        {
            result.Id = Guid.NewGuid();
            result.CreatedAt = DateTime.UtcNow;
            result.UpdatedAt = DateTime.UtcNow;

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetResults), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> UpdateResult(Guid id, AssessmentResult result)
        {
            if (id != result.Id)
            {
                return BadRequest();
            }

            result.UpdatedAt = DateTime.UtcNow;
            _context.Entry(result).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResultExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> DeleteResult(Guid id)
        {
            var result = await _context.Results.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ResultExists(Guid id)
        {
            return _context.Results.Any(e => e.Id == id);
        }
    }
}
