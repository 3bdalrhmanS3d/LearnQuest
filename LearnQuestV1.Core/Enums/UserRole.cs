using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Enums
{
    // ────────────────────────────────────────────────────────────────────────────────
    // 1) UserRole enum (defines possible roles for a user)
    // ────────────────────────────────────────────────────────────────────────────────

    public enum UserRole
    {
        [Description("RegularUser")]
        RegularUser,

        [Description("Instructor")]
        Instructor,

        [Description("Admin")]
        Admin
    }
}
