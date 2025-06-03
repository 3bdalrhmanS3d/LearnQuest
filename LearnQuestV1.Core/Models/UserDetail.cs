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
    public class UserDetail
    {
        public UserDetail()
        {
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Primary key & Foreign key → one-to-one with User.
        /// If you want a truly required 1:1, this must always match an existing UserId.
        /// </summary>
        [Key]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string EducationLevel { get; set; } = string.Empty;
        // (e.g. "Primary", "Middle", "High School", "University")

        [Required]
        [MaxLength(50)]
        public string Nationality { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when this detail row was created.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Navigation back to User.
        /// Since it's 1:1, mark as virtual if you use lazy loading.
        /// </summary>
        public virtual User User { get; set; } = null!;
    }
}
