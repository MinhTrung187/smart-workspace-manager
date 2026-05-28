import { Link } from 'react-router';
import { useWorkspacesQuery } from '../hooks/useWorkspace';
import { LayoutGrid, Plus, Calendar, User, ArrowRight } from 'lucide-react';

interface WorkspaceListProps {
  onCreateClick: () => void;
}

export default function WorkspaceList({ onCreateClick }: WorkspaceListProps) {
  const { data: workspaces, isLoading, isError } = useWorkspacesQuery();
  
  // Ensure workspaces is always an array
  const workspacesList = Array.isArray(workspaces) ? workspaces : [];

  if (isLoading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {[1, 2, 3].map((i) => (
          <div key={i} className="h-48 rounded-xl bg-slate-100 animate-pulse"></div>
        ))}
      </div>
    );
  }

  if (isError) {
    return (
      <div className="p-6 bg-red-50 border border-red-100 rounded-xl text-center">
        <p className="text-red-600 font-medium">Failed to load workspaces. Please try refreshing the page.</p>
      </div>
    );
  }

  if (!workspacesList || workspacesList.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-20 px-4 text-center bg-white border border-slate-200 border-dashed rounded-2xl">
        <div className="w-16 h-16 bg-indigo-50 text-indigo-600 rounded-2xl flex items-center justify-center mb-6 shadow-sm">
          <LayoutGrid className="w-8 h-8" />
        </div>
        <h3 className="text-xl font-bold text-slate-900 mb-2">No workspaces yet</h3>
        <p className="text-slate-500 max-w-sm mx-auto mb-8">
          Get started by creating your first workspace to manage tasks, collaborate with your team, and track progress.
        </p>
        <button
          onClick={onCreateClick}
          className="inline-flex items-center gap-2 px-5 py-2.5 text-sm font-semibold text-white bg-indigo-600 rounded-lg shadow-sm hover:bg-indigo-700 transition-all focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
        >
          <Plus className="w-4 h-4" />
          Create First Workspace
        </button>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {workspacesList.map((ws) => {
        // Generate initials (e.g. "Engineering Team" -> "ET")
        const initials = ws.name
          .split(' ')
          .map((n) => n[0])
          .join('')
          .substring(0, 2)
          .toUpperCase() || 'W';

        // Format dates simply for now
        const dateObj = new Date(ws.createdAt);
        const dateStr = dateObj.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });

        return (
          <Link
            key={ws.id}
            to={`/workspaces/${ws.id}`}
            className="group flex flex-col justify-between bg-white border border-slate-200 rounded-xl p-6 shadow-sm hover:shadow-md hover:border-indigo-200 transition-all focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
          >
            <div>
              <div className="flex items-center justify-between mb-4">
                <div className="w-12 h-12 bg-indigo-100 text-indigo-700 font-bold text-lg rounded-xl flex items-center justify-center shadow-inner">
                  {initials}
                </div>
                <div className="w-8 h-8 rounded-full bg-slate-50 border border-slate-200 flex items-center justify-center text-slate-400 group-hover:text-indigo-600 group-hover:bg-indigo-50 transition-colors">
                  <ArrowRight className="w-4 h-4" />
                </div>
              </div>
              <h3 className="text-lg font-bold text-slate-900 group-hover:text-indigo-600 transition-colors line-clamp-1 mb-2">
                {ws.name}
              </h3>
              <p className="text-sm text-slate-500 line-clamp-2 h-10 mb-4">
                {ws.description || 'No description provided.'}
              </p>
            </div>
            
            <div className="flex items-center gap-4 pt-4 border-t border-slate-100 text-xs font-medium text-slate-500">
              <div className="flex items-center gap-1.5 truncate">
                <User className="w-3.5 h-3.5" />
                <span className="truncate">{ws.ownerName || 'Unknown Owner'}</span>
              </div>
              <div className="flex items-center gap-1.5 shrink-0">
                <Calendar className="w-3.5 h-3.5" />
                <span>{dateStr}</span>
              </div>
            </div>
          </Link>
        );
      })}
    </div>
  );
}
