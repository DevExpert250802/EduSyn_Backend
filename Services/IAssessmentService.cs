using edusync_backend.DTOs;

namespace edusync_backend.Services
{
    public interface IAssessmentService
    {
        Task<IEnumerable<AssessmentDto>> GetAssessmentsByCourseAsync(Guid courseId);
        Task<AssessmentDto> GetAssessmentByIdAsync(Guid id);
        Task<Guid> CreateAssessmentAsync(CreateAssessmentDto dto);
        Task<bool> UpdateAssessmentAsync(UpdateAssessmentDto dto);
        Task<bool> DeleteAssessmentAsync(Guid id);
        Task<Guid> SubmitAssessmentAsync(Guid assessmentId, Guid studentId, SubmitAssessmentDto dto);
        Task<bool> GradeSubmissionAsync(Guid submissionId, GradeSubmissionDto dto);
        Task<IEnumerable<CourseResultDto>> GetAssessmentResultsAsync(Guid assessmentId);
        Task<IEnumerable<CourseResultDto>> GetStudentAssessmentResultsAsync(Guid studentId);
        Task<AssessmentResultDto> GetAssessmentResultAsync(Guid assessmentId, Guid studentId);
    }
} 