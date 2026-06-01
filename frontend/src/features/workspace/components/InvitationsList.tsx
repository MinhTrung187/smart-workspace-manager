import { useMyInvitationsQuery, useAcceptInvitationMutation } from '../hooks/useWorkspaceInvitationMutations';
import { Mail, Check, Calendar, Loader2, Info } from 'lucide-react';
import { useState } from 'react';

export default function InvitationsList() {
  const { data: invitations, isLoading, isError, refetch } = useMyInvitationsQuery();
  const acceptMutation = useAcceptInvitationMutation();
  const [errorMap, setErrorMap] = useState<Record<string, string>>({});

  const handleAccept = (invitationId: string, _workspaceName: string) => {
    acceptMutation.mutate(invitationId, {
      onError: (err: any) => {
        const errorMsg = err.response?.data?.message || 'Failed to accept invitation. Please try again.';
        setErrorMap((prev) => ({ ...prev, [invitationId]: errorMsg }));
      },
      onSuccess: () => {
        setErrorMap((prev) => {
          const next = { ...prev };
          delete next[invitationId];
          return next;
        });
      }
    });
  };

  // Only show invitations with "Pending" or standard active status if applicable, otherwise display all retrieved
  const pendingInvitations = invitations?.filter((invite) => invite.status === 'Pending' || invite.status === 'Active') || [];

  if (isLoading) {
    return (
      <div className="bg-white rounded-2xl border border-slate-200 p-6 flex flex-col items-center justify-center gap-3">
        <Loader2 className="w-5 h-5 text-indigo-600 animate-spin" />
        <span className="text-xs text-slate-500 font-semibold">Checking invitations...</span>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="bg-white rounded-2xl border border-red-100 p-6 text-center">
        <p className="text-sm text-red-600 font-semibold mb-2">Could not load invitations.</p>
        <button
          onClick={() => refetch()}
          className="text-xs font-bold text-indigo-600 hover:underline"
        >
          Try Again
        </button>
      </div>
    );
  }

  if (pendingInvitations.length === 0) {
    return null; // Don't show anything on the dashboard if there are zero invitations
  }

  return (
    <div className="bg-linear-to-br from-indigo-50 to-slate-50 rounded-2xl border border-indigo-100 shadow-sm p-6 mb-8 animate-in fade-in slide-in-from-top-4 duration-300">
      <div className="flex items-center gap-2.5 mb-4">
        <div className="p-2 bg-indigo-600 rounded-lg text-white">
          <Mail className="w-4 h-4" />
        </div>
        <div>
          <h3 className="text-base font-bold text-slate-950 flex items-center gap-2">
            Workspace Invitations
            <span className="inline-flex h-5 w-5 items-center justify-center rounded-full bg-indigo-100 text-[11px] font-bold text-indigo-700">
              {pendingInvitations.length}
            </span>
          </h3>
          <p className="text-xs text-slate-500 font-medium">You have been invited to join the following workspaces.</p>
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {pendingInvitations.map((invite) => {
          const isPendingAccept = acceptMutation.isPending && acceptMutation.variables === invite.id;
          const errorMessage = errorMap[invite.id];

          return (
            <div
              key={invite.id}
              className="bg-white rounded-xl border border-slate-200/80 p-4 shadow-sm hover:shadow-md transition-shadow flex flex-col justify-between gap-4"
            >
              <div className="space-y-1.5">
                <span className="inline-flex items-center px-1.5 py-0.5 rounded text-[9px] font-bold uppercase tracking-wider text-indigo-600 bg-indigo-50 border border-indigo-100/50 leading-normal mb-1">
                  Invitation
                </span>
                <h4 className="text-sm font-extrabold text-slate-900 line-clamp-1">
                  {invite.workspaceName}
                </h4>
                
                <div className="flex items-center gap-1.5 text-xs text-slate-400 font-medium">
                  <Calendar className="w-3.5 h-3.5" />
                  <span>Received {new Date(invite.createdAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}</span>
                </div>

                {errorMessage && (
                  <div className="p-2.5 bg-red-50 border border-red-100 rounded-lg text-[11px] text-red-600 font-medium flex items-start gap-1">
                    <Info className="w-3 h-3 shrink-0 mt-0.5" />
                    <span>{errorMessage}</span>
                  </div>
                )}
              </div>

              <div className="flex gap-2">
                <button
                  onClick={() => handleAccept(invite.id, invite.workspaceName)}
                  disabled={isPendingAccept}
                  className="flex-1 inline-flex items-center justify-center gap-1.5 px-3 py-2 text-xs font-bold text-white bg-indigo-600 hover:bg-indigo-700 rounded-lg transition-all focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-indigo-500 disabled:opacity-60 disabled:cursor-not-allowed"
                >
                  {isPendingAccept ? (
                    <>
                      <Loader2 className="w-3.5 h-3.5 animate-spin" />
                      Accepting...
                    </>
                  ) : (
                    <>
                      <Check className="w-3.5 h-3.5" />
                      Accept
                    </>
                  )}
                </button>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
