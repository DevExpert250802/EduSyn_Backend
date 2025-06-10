using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace edusync_backend.Models
{
    public class Course
    {
        [Key]
        public Guid CourseId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string? Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        public string? MediaUrl { get; set; }

        public Guid? InstructorId { get; set; }
        public User? Instructor { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid? CreatedById { get; set; }
        [ForeignKey("CreatedById")]
        public User? CreatedBy { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
        public ICollection<Announcement> Announcements { get; set; }
        public ICollection<ForumPost> ForumPosts { get; set; }
        public ICollection<CalendarEvent> CalendarEvents { get; set; }
    }
}
