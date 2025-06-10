using Microsoft.EntityFrameworkCore;
using edusync_backend.Models;

namespace edusync_backend.Data
{
    public class EduSyncDbContext : DbContext
    {
        public EduSyncDbContext(DbContextOptions<EduSyncDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<AssessmentQuestion> AssessmentQuestions { get; set; }
        public DbSet<AssessmentSubmission> AssessmentSubmissions { get; set; }
        public DbSet<AssessmentAnswer> AssessmentAnswers { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<ForumPost> ForumPosts { get; set; }
        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<AssessmentResult> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Course entity
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.CourseId);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.MediaUrl).IsRequired();
            });

            // Configure Assessment entity
            modelBuilder.Entity<Assessment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.DueDate).IsRequired();
                entity.Property(e => e.TotalMarks).IsRequired();

                entity.HasOne(a => a.Course)
                    .WithMany(c => c.Assessments)
                    .HasForeignKey(a => a.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure AssessmentQuestion entity
            modelBuilder.Entity<AssessmentQuestion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QuestionText).IsRequired();
                entity.Property(e => e.QuestionType).IsRequired();
                entity.Property(e => e.Marks).IsRequired();

                entity.HasOne(q => q.Assessment)
                    .WithMany(a => a.Questions)
                    .HasForeignKey(q => q.AssessmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure AssessmentSubmission entity
            modelBuilder.Entity<AssessmentSubmission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SubmissionDate).IsRequired();
                entity.Property(e => e.SubmittedAt).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Feedback).HasMaxLength(1000);
                entity.Property(e => e.TotalMarks).HasPrecision(10, 2);
                entity.Property(e => e.Grade).HasPrecision(10, 2);
                entity.Property(e => e.GradedDate);

                entity.HasOne(s => s.Assessment)
                    .WithMany(a => a.Submissions)
                    .HasForeignKey(s => s.AssessmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Student)
                    .WithMany()
                    .HasForeignKey(s => s.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure AssessmentAnswer entity
            modelBuilder.Entity<AssessmentAnswer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Answer).IsRequired();
                entity.Property(e => e.Feedback).HasMaxLength(1000);
                entity.Property(e => e.Marks).HasPrecision(10, 2);

                entity.HasOne(a => a.Submission)
                    .WithMany(s => s.Answers)
                    .HasForeignKey(a => a.AssessmentSubmissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Question)
                    .WithMany(q => q.Answers)
                    .HasForeignKey(a => a.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Enrollment entity
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.EnrollmentId);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Progress).HasDefaultValue(0);

                entity.HasOne(e => e.Course)
                    .WithMany()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Message entity
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.IsRead).IsRequired();

                entity.HasOne(e => e.Sender)
                    .WithMany()
                    .HasForeignKey(e => e.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Receiver)
                    .WithMany()
                    .HasForeignKey(e => e.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                    .WithMany()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Announcement entity
            modelBuilder.Entity<Announcement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsPinned).IsRequired().HasDefaultValue(false);

                entity.HasOne(a => a.Course)
                    .WithMany(c => c.Announcements)
                    .HasForeignKey(a => a.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.CreatedBy)
                    .WithMany()
                    .HasForeignKey(a => a.CreatedById)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure ForumPost entity
            modelBuilder.Entity<ForumPost>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsPinned).IsRequired();
                entity.Property(e => e.IsLocked).IsRequired();

                entity.HasOne(e => e.Course)
                    .WithMany()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ParentPost)
                    .WithMany(e => e.Replies)
                    .HasForeignKey(e => e.ParentPostId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure CalendarEvent entity
            modelBuilder.Entity<CalendarEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.EndTime).IsRequired();
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IsRecurring).IsRequired();

                entity.HasOne(e => e.Course)
                    .WithMany()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CreatedBy)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Ignore EmailSettings as it's a configuration class
            modelBuilder.Ignore<EmailSettings>();
        }
    }
} 