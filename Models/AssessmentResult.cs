using System.ComponentModel.DataAnnotations;

namespace edusync_backend.Models
{
    public class AssessmentResult
    {
        public Guid Id { get; set; }

        [Required]
        public Guid AssessmentId { get; set; }
        public Assessment Assessment { get; set; }

        [Required]
        public Guid StudentId { get; set; }
        public User Student { get; set; }

        [Required]
        public double Score { get; set; }

        [Required]
        public double TotalMarks { get; set; }

        [Required]
        public string Status { get; set; }

        public string? Feedback { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 