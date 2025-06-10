namespace edusync_backend.DTOs
{
    public class CourseResultDto
    {
        public Guid ResultId { get; set; }
        public Guid AssessmentId { get; set; }
        public Guid UserId { get; set; }
        public int Score { get; set; }
        public DateTime AttemptDate { get; set; }

        public string UserName { get; set; }
        public string AssessmentTitle { get; set; }
    }

    public class CreateCourseResultDto
    {
        public Guid AssessmentId { get; set; }
        public Guid UserId { get; set; }
        public int Score { get; set; }
        public DateTime AttemptDate { get; set; }
    }

    public class UpdateCourseResultDto
    {
        public Guid ResultId { get; set; }
        public int Score { get; set; }
        public DateTime AttemptDate { get; set; }
    }
}
