using System.ComponentModel.DataAnnotations;

namespace edusync_backend.Models
{
    public class Enrollment
    {
        public Guid EnrollmentId { get; set; }

        [Required]
        public Guid CourseId { get; set; }
        public Course Course { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; }

        public DateTime EnrollmentDate { get; set; }
        public double Progress { get; set; }
        public string Status { get; set; } // "Active", "Completed", "Dropped"
        public DateTime? CompletionDate { get; set; }
    }
} 