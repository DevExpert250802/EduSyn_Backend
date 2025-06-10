using System;

namespace edusync_backend.DTOs
{
    public class AnnouncementDTO
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid CourseId { get; set; }
        public required string CourseName { get; set; }
        public Guid? CreatedById { get; set; }
        public required string CreatedByName { get; set; }
        public bool IsPinned { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class CreateAnnouncementDTO
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
        public Guid CourseId { get; set; }
        public bool IsPinned { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class AnnouncementResponseDTO
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public required AnnouncementDTO Data { get; set; }
    }
} 