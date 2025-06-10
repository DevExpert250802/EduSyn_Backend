using System;

namespace edusync_backend.DTOs
{
    public class CalendarEventDTO
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public required string EventType { get; set; }
        public string? Location { get; set; }
        public Guid CourseId { get; set; }
        public required string CourseName { get; set; }
        public Guid? CreatedById { get; set; }
        public required string CreatedByName { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
        public string? ReminderSettings { get; set; }
    }

    public class CreateCalendarEventDTO
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public required string EventType { get; set; }
        public string? Location { get; set; }
        public Guid CourseId { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
        public string? ReminderSettings { get; set; }
    }

    public class CalendarEventResponseDTO
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public required CalendarEventDTO Data { get; set; }
    }
} 