namespace edusync_backend.DTOs
{
    public class CourseDTO
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid? InstructorId { get; set; }
        public string MediaUrl { get; set; }
    }

    public class CreateCourseDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid? InstructorId { get; set; }
        public string MediaUrl { get; set; }
    }

    public class UpdateCourseDTO
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid? InstructorId { get; set; }
        public string MediaUrl { get; set; }
    }
}
