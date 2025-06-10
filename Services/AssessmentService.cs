using edusync_backend.Data;
using edusync_backend.DTOs;
using edusync_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace edusync_backend.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly EduSyncDbContext _context;

        public AssessmentService(EduSyncDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AssessmentDto>> GetAssessmentsByCourseAsync(Guid courseId)
        {
            return await _context.Assessments
                .Where(a => a.CourseId == courseId)
                .Select(a => new AssessmentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    DueDate = a.DueDate,
                    CourseId = a.CourseId,
                    CourseName = a.Course.Name,
                    Status = "Not Started",
                    Questions = a.Questions.Select(q => new AssessmentQuestionDto
                    {
                        Id = q.Id,
                        QuestionText = q.QuestionText,
                        QuestionType = q.QuestionType,
                        Options = q.Options,
                        CorrectAnswer = q.CorrectAnswer
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<AssessmentDto> GetAssessmentByIdAsync(Guid id)
        {
            var assessment = await _context.Assessments
                .Include(a => a.Course)
                .Include(a => a.Questions)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assessment == null)
                return null;

            return new AssessmentDto
            {
                Id = assessment.Id,
                Title = assessment.Title,
                Description = assessment.Description,
                DueDate = assessment.DueDate,
                CourseId = assessment.CourseId,
                CourseName = assessment.Course.Name,
                Status = "Not Started",
                Questions = assessment.Questions.Select(q => new AssessmentQuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    Options = q.Options,
                    CorrectAnswer = q.CorrectAnswer
                }).ToList()
            };
        }

        public async Task<Guid> CreateAssessmentAsync(CreateAssessmentDto dto)
        {
            var assessment = new Assessment
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                CourseId = dto.CourseId,
                TotalMarks = dto.Questions.Count * 10, // Assuming 10 marks per question
                CreatedAt = DateTime.UtcNow,
                Questions = dto.Questions.Select(q => new AssessmentQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    Options = q.Options,
                    CorrectAnswer = q.CorrectAnswer,
                    Marks = 10, // Default marks per question
                    AssessmentId = Guid.NewGuid(), // Will be updated after assessment is saved
                    Assessment = null, // Will be set after assessment is saved
                    Answers = new List<AssessmentAnswer>()
                }).ToList(),
                Submissions = new List<AssessmentSubmission>()
            };

            _context.Assessments.Add(assessment);
            await _context.SaveChangesAsync();

            // Update the AssessmentId and Assessment reference for each question
            foreach (var question in assessment.Questions)
            {
                question.AssessmentId = assessment.Id;
                question.Assessment = assessment;
            }
            await _context.SaveChangesAsync();

            return assessment.Id;
        }

        public async Task<bool> UpdateAssessmentAsync(UpdateAssessmentDto dto)
        {
            var assessment = await _context.Assessments
                .Include(a => a.Questions)
                .FirstOrDefaultAsync(a => a.Id == dto.Id);

            if (assessment == null)
                return false;

            assessment.Title = dto.Title;
            assessment.Description = dto.Description;
            assessment.DueDate = dto.DueDate;
            assessment.TotalMarks = dto.Questions.Count * 10; // Assuming 10 marks per question
            assessment.UpdatedAt = DateTime.UtcNow;

            // Update existing questions and add new ones
            var existingQuestionIds = assessment.Questions.Select(q => q.Id).ToList();
            var updatedQuestionIds = dto.Questions.Select(q => q.Id).ToList();

            // Remove questions that are no longer in the update
            var questionsToRemove = assessment.Questions
                .Where(q => !updatedQuestionIds.Contains(q.Id))
                .ToList();
            foreach (var question in questionsToRemove)
            {
                assessment.Questions.Remove(question);
            }

            // Update existing questions and add new ones
            foreach (var questionDto in dto.Questions)
            {
                var existingQuestion = assessment.Questions.FirstOrDefault(q => q.Id == questionDto.Id);
                if (existingQuestion != null)
                {
                    existingQuestion.QuestionText = questionDto.QuestionText;
                    existingQuestion.QuestionType = questionDto.QuestionType;
                    existingQuestion.Options = questionDto.Options;
                    existingQuestion.CorrectAnswer = questionDto.CorrectAnswer;
                }
                else
                {
                    assessment.Questions.Add(new AssessmentQuestion
                    {
                        Id = Guid.NewGuid(),
                        QuestionText = questionDto.QuestionText,
                        QuestionType = questionDto.QuestionType,
                        Options = questionDto.Options,
                        CorrectAnswer = questionDto.CorrectAnswer,
                        Marks = 10, // Default marks per question
                        AssessmentId = assessment.Id,
                        Assessment = assessment,
                        Answers = new List<AssessmentAnswer>()
                    });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAssessmentAsync(Guid id)
        {
            var assessment = await _context.Assessments.FindAsync(id);
            if (assessment == null)
                return false;

            _context.Assessments.Remove(assessment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Guid> SubmitAssessmentAsync(Guid assessmentId, Guid studentId, SubmitAssessmentDto dto)
        {
            var assessment = await _context.Assessments
                .Include(a => a.Questions)
                .FirstOrDefaultAsync(a => a.Id == assessmentId);

            if (assessment == null)
                throw new ArgumentException("Assessment not found");

            var student = await _context.Users.FindAsync(studentId);
            if (student == null)
                throw new ArgumentException("Student not found");

            // Validate that all questions have answers
            var questionIds = assessment.Questions.Select(q => q.Id).ToList();
            var answeredQuestionIds = dto.Answers.Select(a => a.QuestionId).ToList();
            var unansweredQuestions = questionIds.Except(answeredQuestionIds).ToList();
            
            if (unansweredQuestions.Any())
                throw new ArgumentException($"Missing answers for questions: {string.Join(", ", unansweredQuestions)}");

            var now = DateTime.UtcNow;
            var submission = new AssessmentSubmission
            {
                Id = Guid.NewGuid(),
                AssessmentId = assessmentId,
                Assessment = assessment,
                StudentId = studentId,
                Student = student,
                SubmissionDate = now,
                SubmittedAt = now,
                Status = "Submitted",
                TotalMarks = assessment.TotalMarks,
                Answers = new List<AssessmentAnswer>()
            };

            decimal totalMarks = 0;
            bool allQuestionsAutoGraded = true;

            // Create all answers first
            foreach (var answerDto in dto.Answers)
            {
                if (string.IsNullOrWhiteSpace(answerDto.Answer))
                    throw new ArgumentException($"Answer cannot be empty for question {answerDto.QuestionId}");

                var question = assessment.Questions.FirstOrDefault(q => q.Id == answerDto.QuestionId);
                if (question == null)
                    throw new ArgumentException($"Question with ID {answerDto.QuestionId} not found");

                decimal marks = 0;
                string feedback = "";

                // Auto-grade multiple choice and true/false questions
                if (question.QuestionType == "MultipleChoice" || question.QuestionType == "TrueFalse")
                {
                    if (answerDto.Answer.Trim().Equals(question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        marks = question.Marks;
                        feedback = "Correct answer";
                    }
                    else
                    {
                        feedback = $"Incorrect. The correct answer was: {question.CorrectAnswer}";
                    }
                    totalMarks += marks;
                }
                else
                {
                    // Essay and short answer questions need manual grading
                    allQuestionsAutoGraded = false;
                    feedback = "Pending manual grading";
                }

                var answer = new AssessmentAnswer
                {
                    Id = Guid.NewGuid(),
                    Answer = answerDto.Answer.Trim(),
                    QuestionId = answerDto.QuestionId,
                    Question = question,
                    AssessmentSubmissionId = submission.Id,
                    Submission = submission,
                    Marks = marks,
                    Feedback = feedback
                };

                submission.Answers.Add(answer);
            }

            // If all questions were auto-graded, update the submission status and grade
            if (allQuestionsAutoGraded)
            {
                submission.Status = "Graded";
                submission.Grade = totalMarks;
                submission.GradedDate = now;
                submission.Feedback = $"Auto-graded. Total marks: {totalMarks} out of {assessment.TotalMarks}";
            }

            // Add the submission with all its answers
            _context.AssessmentSubmissions.Add(submission);
            
            // Save everything in a single transaction
            await _context.SaveChangesAsync();
            
            return submission.Id;
        }

        public async Task<bool> GradeSubmissionAsync(Guid submissionId, GradeSubmissionDto dto)
        {
            var submission = await _context.AssessmentSubmissions
                .Include(s => s.Answers)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                return false;

            submission.Grade = dto.Grade;
            submission.Feedback = dto.Feedback;
            submission.Status = "Graded";
            submission.GradedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CourseResultDto>> GetAssessmentResultsAsync(Guid assessmentId)
        {
            return await _context.AssessmentSubmissions
                .Where(s => s.AssessmentId == assessmentId)
                .Select(s => new CourseResultDto
                {
                    ResultId = s.Id,
                    AssessmentId = s.AssessmentId,
                    UserId = s.StudentId,
                    Score = (int)(s.Grade ?? 0),
                    AttemptDate = s.SubmissionDate,
                    UserName = s.Student.Name,
                    AssessmentTitle = s.Assessment.Title
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<CourseResultDto>> GetStudentAssessmentResultsAsync(Guid studentId)
        {
            return await _context.AssessmentSubmissions
                .Where(s => s.StudentId == studentId)
                .Select(s => new CourseResultDto
                {
                    ResultId = s.Id,
                    AssessmentId = s.AssessmentId,
                    UserId = s.StudentId,
                    Score = (int)(s.Grade ?? 0),
                    AttemptDate = s.SubmissionDate,
                    UserName = s.Student.Name,
                    AssessmentTitle = s.Assessment.Title
                })
                .ToListAsync();
        }

        public async Task<AssessmentResultDto> GetAssessmentResultAsync(Guid assessmentId, Guid studentId)
        {
            try
            {
                var submission = await _context.AssessmentSubmissions
                    .Include(s => s.Student)
                    .Include(s => s.Assessment)
                        .ThenInclude(a => a.Course)
                    .Include(s => s.Answers)
                        .ThenInclude(a => a.Question)
                    .FirstOrDefaultAsync(s => s.AssessmentId == assessmentId && s.StudentId == studentId);

                if (submission == null)
                    throw new ArgumentException($"No submission found for assessment {assessmentId} and student {studentId}");

                if (submission.Assessment == null)
                    throw new ArgumentException($"Assessment {assessmentId} not found");

                if (submission.Student == null)
                    throw new ArgumentException($"Student {studentId} not found");

                var totalQuestions = submission.Assessment.Questions.Count;
                var score = submission.Grade.HasValue 
                    ? (int)((submission.Grade.Value / submission.TotalMarks) * 100) 
                    : 0;

                return new AssessmentResultDto
                {
                    SubmissionId = submission.Id,
                    StudentId = submission.StudentId,
                    StudentName = submission.Student.Name,
                    Grade = submission.Grade ?? 0,
                    Score = score,
                    Feedback = submission.Feedback ?? "No feedback provided",
                    SubmissionDate = submission.SubmissionDate,
                    GradedDate = submission.GradedDate,
                    Status = submission.Status,
                    Assessment = new AssessmentDto
                    {
                        Id = submission.Assessment.Id,
                        Title = submission.Assessment.Title,
                        Description = submission.Assessment.Description,
                        CourseId = submission.Assessment.CourseId,
                        CourseName = submission.Assessment.Course.Name,
                        DueDate = submission.Assessment.DueDate,
                        Status = submission.Status,
                        Questions = submission.Answers.Select(a => new AssessmentQuestionDto
                        {
                            Id = a.QuestionId,
                            QuestionText = a.Question.QuestionText,
                            QuestionType = a.Question.QuestionType,
                            Options = a.Question.Options,
                            CorrectAnswer = a.Question.CorrectAnswer,
                            UserAnswer = a.Answer,
                            IsCorrect = a.Answer.Trim().Equals(a.Question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase),
                            Score = a.Marks > 0 ? (int)((a.Marks / a.Question.Marks) * 100) : 0
                        }).ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAssessmentResultAsync: {ex}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException}");
                }
                throw;
            }
        }
    }
} 