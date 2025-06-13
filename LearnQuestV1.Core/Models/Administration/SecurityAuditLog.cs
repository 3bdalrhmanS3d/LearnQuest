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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? EmailAttempted { get; set; }
        public string? IpAddress { get; set; }
        public bool? Success { get; set; }
        public string? FailureReason { get; set; }
        public string EventType { get; set; } = null!;
        public string? EventDetails { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
