using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnQuestV1.Core.Enums
{
    /// <summary>
    /// Predefined notification templates that an administrator can send to users.
    /// </summary>
    public enum NotificationTemplateType
    {
        /// <summary>
        /// “Your account has been activated” message.
        /// </summary>
        AccountActivated = 0,

        /// <summary>
        /// “Your account has been deactivated” message.
        /// </summary>
        AccountDeactivated = 1,

        /// <summary>
        /// “Your account has been deleted” message.
        /// </summary>
        AccountDeleted = 2,

        /// <summary>
        /// “Your account has been restored” message.
        /// </summary>
        AccountRestored = 3,

        /// <summary>
        /// A general announcement/alert (non‐account‐specific).
        /// </summary>
        GeneralAnnouncement = 4
    }
}
