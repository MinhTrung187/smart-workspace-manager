import { useParams, Link } from 'react-router';
import { useState } from 'react';
import { useWorkspaceDetailQuery } from '../features/workspace/hooks/useWorkspace';
import BoardGrid from '../features/board/components/BoardGrid';
import { LayoutTemplate, ArrowLeft, UserPlus, Users } from 'lucide-react';
import InviteMemberModal from '../features/workspace/components/InviteMemberModal';
import WorkspaceMembersModal from '../features/workspace/components/WorkspaceMembersModal';


export default function WorkspaceDetail() {
  const { id } = useParams<{ id: string }>();
  const { data: workspace, isLoading, isError } = useWorkspaceDetailQuery(id || '');
  const [isInviteModalOpen, setIsInviteModalOpen] = useState(false);
  const [isMembersModalOpen, setIsMembersModalOpen] = useState(false);
  if (isLoading) {
    return (
      <div className="min-h-screen bg-slate-50 p-8 flex items-center justify-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  if (isError || !workspace) {
    return (
      <div className="min-h-screen bg-slate-50 p-8">
        <div className="max-w-4xl mx-auto bg-white p-6 rounded-xl border border-red-100 shadow-sm text-center">
          <p className="text-red-600 mb-4">Workspace not found or failed to load.</p>
          <Link to="/dashboard" className="text-indigo-600 hover:underline font-medium">Return to Dashboard</Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50 flex flex-col">
      <header className="bg-white border-b border-slate-200 sticky top-0 z-10">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex h-16 items-center gap-4">
            <Link to="/dashboard" className="p-2 text-slate-400 hover:text-slate-600 hover:bg-slate-50 rounded-lg transition-colors">
              <ArrowLeft className="w-5 h-5" />
            </Link>
            <div className="h-6 border-l border-slate-200"></div>
            <div className="flex items-center gap-2">
              <div className="p-1.5 bg-indigo-600 rounded-lg">
                <LayoutTemplate className="w-4 h-4 text-white" />
              </div>
              <span className="text-sm font-semibold text-slate-500">Workspace / </span>
              <h1 className="text-lg font-bold text-slate-900 tracking-tight">{workspace.name}</h1>
            </div>
          </div>
        </div>
      </header>

      <main className="flex-1 p-4 sm:p-6 lg:p-8 max-w-7xl w-full mx-auto">
        <div className="bg-white p-6 sm:p-8 rounded-2xl border border-slate-200 shadow-sm mb-8">
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-4">
            <h2 className="text-2xl font-bold text-slate-900 leading-none">{workspace.name}</h2>
            <button
              onClick={() => setIsInviteModalOpen(true)}
              className="inline-flex items-center justify-center gap-2 px-4 py-2 text-sm font-semibold text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 rounded-lg shadow-sm transition-all self-start sm:self-auto"
            >
              <UserPlus className="w-4 h-4" />
              Invite Member
            </button>
          </div>
          <p className="text-slate-500 max-w-3xl leading-relaxed mb-6">
            {workspace.description || 'No description provided for this workspace.'}
          </p>
          <div className="flex items-center gap-6 text-sm font-medium text-slate-500 border-t border-slate-100 pt-6">
            <div className="flex flex-col">
              <span className="text-xs text-slate-400 mb-1">Owner</span>
              <span className="text-slate-700">{workspace.ownerName || 'Unknown Owner'}</span>
            </div>
                         <div className="h-8 border-l border-slate-200"></div>
             <button
               onClick={() => setIsMembersModalOpen(true)}
               className="inline-flex items-center gap-1.5 px-3.5 py-1.5 bg-slate-50 hover:bg-slate-200/60 text-slate-700 text-xs font-semibold rounded-full border border-slate-200 shadow-sm transition-all focus:outline-none cursor-pointer group hover:text-slate-950"
             >
               <Users className="w-3.5 h-3.5 text-slate-400 group-hover:text-indigo-600 transition-colors" />
               <span>Members: <span className="text-indigo-600 font-bold">{workspace.memberCount ?? 0}</span></span>
             </button>
            <div className="h-8 border-l border-slate-200"></div>
            <div className="flex flex-col">
              <span className="text-xs text-slate-400 mb-1">Created At</span>
              <span className="text-slate-700">{new Date(workspace.createdAt).toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' })}</span>
            </div>
          </div>
        </div>

        <div className="mb-6">
          <h2 className="text-xl font-bold text-slate-900">Workspace Boards</h2>
          <p className="text-sm text-slate-500 mt-1">Select a board to view its columns and manage tasks.</p>
        </div>

        <BoardGrid workspaceId={workspace.id} boards={workspace.boards || []} />
      </main>
            <InviteMemberModal
        isOpen={isInviteModalOpen}
        onClose={() => setIsInviteModalOpen(false)}
        workspaceId={workspace.id}
        workspaceName={workspace.name}
      />
            <WorkspaceMembersModal
        isOpen={isMembersModalOpen}
        onClose={() => setIsMembersModalOpen(false)}
        workspaceId={workspace.id}
      />
    </div>
  );
}
