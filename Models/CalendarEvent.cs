using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace edusync_backend.Models
{
    public class CalendarEvent
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public string EventType { get; set; } // Class, Assignment, Exam, etc.

        public string Location { get; set; }

        [Required]
        public Guid CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        public Guid? CreatedById { get; set; }
        [ForeignKey("CreatedById")]
        public User CreatedBy { get; set; }

        public bool IsRecurring { get; set; }
        public string RecurrencePattern { get; set; } // JSON string for recurrence rules
        public DateTime? RecurrenceEndDate { get; set; }

        public string ReminderSettings { get; set; } // JSON string for reminder settings
    }
} 