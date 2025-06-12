using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Models.UserManagement
{
    [Table("UserVisitHistory")]
    public class UserVisitHistory
    {
        public UserVisitHistory()
        {
            LastVisit = DateTime.UtcNow;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// FK → the user who visited.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// UTC timestamp when the user’s last visit was recorded.
        /// </summary>
        [Required]
        public DateTime LastVisit { get; set; }

        /// <summary>
        /// Navigation back to User.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}
