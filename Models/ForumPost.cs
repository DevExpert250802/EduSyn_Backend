using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace edusync_backend.Models
{
    public class ForumPost
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool IsPinned { get; set; }
        public bool IsLocked { get; set; }

        public Guid CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        public Guid AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public User Author { get; set; }

        public Guid? ParentPostId { get; set; }
        [ForeignKey("ParentPostId")]
        public ForumPost? ParentPost { get; set; }

        public ICollection<ForumPost> Replies { get; set; }
    }
} 