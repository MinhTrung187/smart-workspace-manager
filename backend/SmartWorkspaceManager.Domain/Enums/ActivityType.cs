using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWorkspaceManager.Domain.Enums
{
    public enum ActivityType
    {
        WorkspaceCreated = 0,
        MemberInvited = 1,
        BoardCreated = 2,
        BoardUpdated = 3,
        TaskCreated = 4,
        TaskUpdated = 5,
        TaskAssigned = 6,
        TaskCommented = 7,
        TaskAttachmentUploaded = 8,
        DueDateChanged = 9
    }
}
