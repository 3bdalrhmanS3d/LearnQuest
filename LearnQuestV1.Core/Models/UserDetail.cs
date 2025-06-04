using LearnQuestV1.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Models
{
    [Table("UserDetails")]
    public class UserDetail
    {
        public UserDetail()
        {
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Primary key & foreign key → enforces one-to-one with User.
        /// </summary>
        [Key]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string EducationLevel { get; set; } = string.Empty;
        // e.g.: "Primary", "Middle", "High School", "University"

        [Required]
        [MaxLength(50)]
        public string Nationality { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when this detail record was created.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Navigation back to the related User.
        /// </summary>
        public virtual User User { get; set; } = null!;
    }
}
