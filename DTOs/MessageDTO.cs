using System;

namespace edusync_backend.DTOs
{
    public class MessageDTO
    {
        public Guid Id { get; set; }
        public required string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
        public Guid SenderId { get; set; }
        public required string SenderName { get; set; }
        public Guid ReceiverId { get; set; }
        public required string ReceiverName { get; set; }
        public Guid CourseId { get; set; }
        public required string CourseName { get; set; }
    }

    public class CreateMessageDTO
    {
        public required string Content { get; set; }
        public Guid ReceiverId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class MessageResponseDTO
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public required MessageDTO Data { get; set; }
    }
} 