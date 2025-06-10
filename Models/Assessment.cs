using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace edusync_backend.Models
{
    public class Assessment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public decimal TotalMarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public Guid CourseId { get; set; }

        // Navigation properties
        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        public List<AssessmentQuestion> Questions { get; set; } = new();
        public List<AssessmentSubmission> Submissions { get; set; } = new();
    }

    public class AssessmentQuestion
    {
        public Guid Id { get; set; }
        public required string QuestionText { get; set; }
        public required string QuestionType { get; set; }
        public required List<string> Options { get; set; }
        public required string CorrectAnswer { get; set; }
        public decimal Marks { get; set; } = 10; // Default marks per question
        public Guid AssessmentId { get; set; }
        public required Assessment Assessment { get; set; }
        public required List<AssessmentAnswer> Answers { get; set; } = new();
    }

    public class AssessmentSubmission
    {
        public Guid Id { get; set; }
        public DateTime SubmissionDate { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public Guid AssessmentId { get; set; }
        public required Assessment Assessment { get; set; }
        public Guid StudentId { get; set; }
        public required User Student { get; set; }
        public required List<AssessmentAnswer> Answers { get; set; } = new();
        public decimal? Grade { get; set; }
        public decimal TotalMarks { get; set; }
        public string? Feedback { get; set; }
        public DateTime? GradedDate { get; set; }
        public required string Status { get; set; }
    }

    public class AssessmentAnswer
    {
        public Guid Id { get; set; }
        public required string Answer { get; set; }
        public Guid AssessmentSubmissionId { get; set; }
        public required AssessmentSubmission Submission { get; set; }
        public Guid QuestionId { get; set; }
        public required AssessmentQuestion Question { get; set; }
        public decimal Marks { get; set; }
        public string? Feedback { get; set; }
    }
}
