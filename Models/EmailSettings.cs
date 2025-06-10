using System.ComponentModel.DataAnnotations;

namespace edusync_backend.Models
{
    public class EmailSettings
    {
        [Required]
        public string SmtpServer { get; set; } = string.Empty;

        [Required]
        public int SmtpPort { get; set; }

        [Required]
        public string SmtpUsername { get; set; } = string.Empty;

        [Required]
        public string SmtpPassword { get; set; } = string.Empty;

        [Required]
        public string FromEmail { get; set; } = string.Empty;

        [Required]
        public string FromName { get; set; } = string.Empty;

        public bool EnableSsl { get; set; }
    }
} 