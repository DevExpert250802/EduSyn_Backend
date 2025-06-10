using System.ComponentModel.DataAnnotations;

namespace edusync_backend.DTOs
{
    public class AssessmentDto
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime DueDate { get; set; }
        public Guid CourseId { get; set; }
        public required string CourseName { get; set; }
        public required string Status { get; set; }
        public required List<AssessmentQuestionDto> Questions { get; set; }
    }

    public class CreateAssessmentDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public DateTime DueDate { get; set; }
        
        [Required]
        public Guid CourseId { get; set; }
        
        [Required]
        public List<CreateAssessmentQuestionDto> Questions { get; set; } = new();
    }

    public class UpdateAssessmentDto
    {
        public Guid Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public DateTime DueDate { get; set; }
        
        [Required]
        public List<UpdateAssessmentQuestionDto> Questions { get; set; } = new();
    }

    public class AssessmentQuestionDto
    {
        public Guid Id { get; set; }
        public required string QuestionText { get; set; }
        public required string QuestionType { get; set; }
        public required List<string> Options { get; set; }
        public required string CorrectAnswer { get; set; }
        public string? UserAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public int Score { get; set; }
    }

    public class CreateAssessmentQuestionDto
    {
        [Required]
        public string QuestionText { get; set; } = string.Empty;
        
        [Required]
        public string QuestionType { get; set; } = string.Empty;
        
        [Required]
        public List<string> Options { get; set; } = new();
        
        [Required]
        public string CorrectAnswer { get; set; } = string.Empty;
    }

    public class UpdateAssessmentQuestionDto
    {
        public Guid Id { get; set; }
        
        [Required]
        public string QuestionText { get; set; } = string.Empty;
        
        [Required]
        public string QuestionType { get; set; } = string.Empty;
        
        [Required]
        public List<string> Options { get; set; } = new();
        
        [Required]
        public string CorrectAnswer { get; set; } = string.Empty;
    }

    public class SubmitAssessmentDto
    {
        [Required]
        public List<SubmitAnswerDto> Answers { get; set; } = new();
    }

    public class SubmitAnswerDto
    {
        [Required]
        public Guid QuestionId { get; set; }
        
        [Required]
        public string Answer { get; set; } = string.Empty;
    }

    public class GradeSubmissionDto
    {
        [Required]
        public decimal Grade { get; set; }
        
        [Required]
        public string Feedback { get; set; } = string.Empty;
    }

    public class AssessmentResultDto
    {
        public Guid SubmissionId { get; set; }
        public Guid StudentId { get; set; }
        public required string StudentName { get; set; }
        public decimal Grade { get; set; }
        public int Score { get; set; }
        public required string Feedback { get; set; }
        public DateTime SubmissionDate { get; set; }
        public DateTime? GradedDate { get; set; }
        public required string Status { get; set; }
        public required AssessmentDto Assessment { get; set; }
    }
} 