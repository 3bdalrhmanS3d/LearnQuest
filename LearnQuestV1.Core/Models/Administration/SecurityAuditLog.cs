using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Models.Administration
{
    [Table("SecurityAuditLogs")]
    public class SecurityAuditLog
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string? EmailAttempted { get; set; }

        public int? UserId { get; set; }

        public virtual User? User { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [Required]
        public bool Success { get; set; }

        [MaxLength(500)]
        public string? FailureReason { get; set; }

        [Required, MaxLength(50)]
        public string EventType { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? EventDetails { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [MaxLength(100)]
        public string? SessionId { get; set; }

        public int RiskScore { get; set; } = 0;

        [MaxLength(255)]
        public string? GeoLocation { get; set; }
    }



}
