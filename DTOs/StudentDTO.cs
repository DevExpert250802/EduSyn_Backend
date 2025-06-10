using System;

namespace edusync_backend.DTOs
{
    public class StudentDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public double Progress { get; set; }
        public string Status { get; set; }
    }
} 