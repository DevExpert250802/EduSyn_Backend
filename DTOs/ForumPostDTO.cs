using System;
using System.Collections.Generic;

namespace edusync_backend.DTOs
{
    public class ForumPostDTO
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid CourseId { get; set; }
        public required string CourseName { get; set; }
        public Guid AuthorId { get; set; }
        public required string AuthorName { get; set; }
        public bool IsPinned { get; set; }
        public bool IsLocked { get; set; }
        public Guid? ParentPostId { get; set; }
        public ICollection<ForumPostDTO> Replies { get; set; }
    }

    public class CreateForumPostDTO
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
        public Guid CourseId { get; set; }
        public Guid? ParentPostId { get; set; }
    }

    public class ForumPostResponseDTO
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public required ForumPostDTO Data { get; set; }
    }
} 