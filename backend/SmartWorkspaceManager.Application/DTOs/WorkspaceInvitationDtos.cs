using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWorkspaceManager.Application.DTOs
{
    public sealed record WorkspaceInvitationResponse(
    Guid Id,
    Guid WorkspaceId,
    string Email,
    string Status,
    DateTime ExpiredAt,
    string InviteLink
);
    public sealed record AllInvitationResponse(
     Guid Id,
     Guid WorkspaceId,
     string? WorkspaceName,
     string Email,
     string Status,
     DateTime CreatedAt,
     DateTime? ExpiredAt,
     string Link
 );
}
