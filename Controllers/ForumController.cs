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
using System.Security.Claims;

namespace edusync_backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ForumController : ControllerBase
    {
        private readonly EduSyncDbContext _context;

        public ForumController(EduSyncDbContext context)
        {
            _context = context;
        }

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<ForumPostDTO>>> GetCoursePosts(string courseId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var courseGuid = Guid.Parse(courseId);

            var course = await _context.Courses.FindAsync(courseGuid);
            if (course == null)
            {
                return NotFound("Course not found");
            }

            var posts = await _context.ForumPosts
                .Where(p => p.CourseId == courseGuid && p.ParentPostId == null)
                .OrderByDescending(p => p.IsPinned)
                .ThenByDescending(p => p.CreatedAt)
                .Select(p => new ForumPostDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    CourseId = p.CourseId,
                    CourseName = course.Name,
                    AuthorId = p.AuthorId,
                    AuthorName = p.Author.Name,
                    IsPinned = p.IsPinned,
                    IsLocked = p.IsLocked,
                    ParentPostId = p.ParentPostId,
                    Replies = p.Replies.Select(r => new ForumPostDTO
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Content = r.Content,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                        CourseId = r.CourseId,
                        CourseName = course.Name,
                        AuthorId = r.AuthorId,
                        AuthorName = r.Author.Name,
                        IsPinned = r.IsPinned,
                        IsLocked = r.IsLocked,
                        ParentPostId = r.ParentPostId
                    }).ToList()
                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ForumPostDTO>> GetPost(string id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var postId = Guid.Parse(id);

            var post = await _context.ForumPosts
                .Include(p => p.Replies)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                return NotFound();

            if (post.AuthorId != userId)
                return Forbid();

            var postDto = new ForumPostDTO
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                CourseId = post.CourseId,
                CourseName = post.Course.Name,
                AuthorId = post.AuthorId,
                AuthorName = post.Author.Name,
                ParentPostId = post.ParentPostId,
                IsPinned = post.IsPinned,
                IsLocked = post.IsLocked,
                Replies = post.Replies.Select(r => new ForumPostDTO
                {
                    Id = r.Id,
                    Title = r.Title,
                    Content = r.Content,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    CourseId = r.CourseId,
                    CourseName = post.Course.Name,
                    AuthorId = r.AuthorId,
                    AuthorName = r.Author.Name,
                    ParentPostId = r.ParentPostId
                }).ToList()
            };

            return Ok(postDto);
        }

        [HttpPost]
        public async Task<ActionResult<ForumPostDTO>> CreatePost(CreateForumPostDTO postDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var course = await _context.Courses.FindAsync(postDto.CourseId);
            if (course == null)
            {
                return NotFound("Course not found");
            }

            var post = new ForumPost
            {
                Title = postDto.Title,
                Content = postDto.Content,
                CreatedAt = DateTime.UtcNow,
                CourseId = postDto.CourseId,
                AuthorId = userId,
                ParentPostId = postDto.ParentPostId
            };

            _context.ForumPosts.Add(post);
            await _context.SaveChangesAsync();

            var author = await _context.Users.FindAsync(userId);
            if (author == null)
            {
                return NotFound("Author not found");
            }

            var responseDto = new ForumPostDTO
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                CourseId = post.CourseId,
                CourseName = course.Name,
                AuthorId = post.AuthorId,
                AuthorName = author.Name,
                IsPinned = post.IsPinned,
                IsLocked = post.IsLocked,
                ParentPostId = post.ParentPostId,
                Replies = new List<ForumPostDTO>()
            };

            return Ok(responseDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ForumPostDTO>> UpdatePost(string id, CreateForumPostDTO postDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var postId = Guid.Parse(id);

            var post = await _context.ForumPosts.FindAsync(postId);
            if (post == null)
            {
                return NotFound("Post not found");
            }

            if (post.AuthorId != userId)
            {
                return Forbid();
            }

            post.Title = postDto.Title;
            post.Content = postDto.Content;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var course = await _context.Courses.FindAsync(post.CourseId);
            var author = await _context.Users.FindAsync(post.AuthorId);

            if (course == null || author == null)
            {
                return NotFound("Course or author not found");
            }

            var responseDto = new ForumPostDTO
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                CourseId = post.CourseId,
                CourseName = course.Name,
                AuthorId = post.AuthorId,
                AuthorName = author.Name,
                IsPinned = post.IsPinned,
                IsLocked = post.IsLocked,
                ParentPostId = post.ParentPostId,
                Replies = new List<ForumPostDTO>()
            };

            return Ok(responseDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(string id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var postId = Guid.Parse(id);

            var post = await _context.ForumPosts.FindAsync(postId);
            if (post == null)
            {
                return NotFound("Post not found");
            }

            if (post.AuthorId != userId)
            {
                return Forbid();
            }

            _context.ForumPosts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/pin")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult> TogglePin(string id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var postId = Guid.Parse(id);

            var post = await _context.ForumPosts.FindAsync(postId);
            if (post == null)
                return NotFound();

            if (post.AuthorId != userId)
                return Forbid();

            post.IsPinned = !post.IsPinned;
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = $"Post {(post.IsPinned ? "pinned" : "unpinned")} successfully" });
        }

        [HttpPut("{id}/lock")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult> ToggleLock(string id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var postId = Guid.Parse(id);

            var post = await _context.ForumPosts.FindAsync(postId);
            if (post == null)
                return NotFound();

            if (post.AuthorId != userId)
                return Forbid();

            post.IsLocked = !post.IsLocked;
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = $"Post {(post.IsLocked ? "locked" : "unlocked")} successfully" });
        }
    }
} 