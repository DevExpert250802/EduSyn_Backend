using System;

namespace edusync_backend.DTOs
{
    public class EnrollmentDTO
    {
        public Guid EnrollmentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public double Progress { get; set; }
        public required string Status { get; set; }
        public DateTime? CompletionDate { get; set; }
        public CourseDTO Course { get; set; }
    }

    public class CreateEnrollmentDTO
    {
        public Guid CourseId { get; set; }
    }

    public class UpdateProgressDTO
    {
        public double Progress { get; set; }
    }
} 