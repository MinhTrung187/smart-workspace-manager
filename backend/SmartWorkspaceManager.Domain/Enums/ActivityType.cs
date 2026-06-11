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
        MemberJoined = 2,
        MemberRemoved = 3,

        BoardCreated = 4,
        BoardUpdated = 5,

        ColumnCreated = 6,
        ColumnUpdated = 7,
        ColumnMoved = 8,
        ColumnDeleted = 9,

        TaskCreated = 10,
        TaskUpdated = 11,
        TaskDeleted = 12,
        TaskMoved = 13,

        TaskAssigned = 14,
        TaskUnassigned = 15,

        TaskCommented = 16,
        TaskAttachmentUploaded = 17,

        DueDateChanged = 18
    }
}
