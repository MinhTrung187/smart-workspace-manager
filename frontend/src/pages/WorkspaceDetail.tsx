import { useParams, Link } from 'react-router';
import { useWorkspaceDetailQuery } from '../features/workspace/hooks/useWorkspace';
import { LayoutTemplate, ArrowLeft } from 'lucide-react';

export default function WorkspaceDetail() {
  const { id } = useParams<{ id: string }>();
  const { data: workspace, isLoading, isError } = useWorkspaceDetailQuery(id || '');

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
          <Link to="/dashboard" className="text-indigo-600 hover:underline">Return to Dashboard</Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50 flex flex-col">
      <header className="bg-white border-b border-slate-200">
        <div className="max-w-400 mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex h-16 items-center gap-4">
            <Link to="/dashboard" className="p-2 text-slate-400 hover:text-slate-600 hover:bg-slate-50 rounded-lg transition-colors">
              <ArrowLeft className="w-5 h-5" />
            </Link>
            <div className="h-6 border-l border-slate-200"></div>
            <div className="flex items-center gap-2">
              <div className="p-1.5 bg-indigo-600 rounded-lg">
                  <LayoutTemplate className="w-4 h-4 text-white" />
              </div>
              <h1 className="text-lg font-bold text-slate-900 tracking-tight">{workspace.name}</h1>
            </div>
          </div>
        </div>
      </header>

      <main className="flex-1 overflow-x-auto p-4 sm:p-6 lg:p-8 max-w-400 w-full mx-auto">
        <div className="mb-6">
          <h2 className="text-xl font-semibold text-slate-900">Boards</h2>
          <p className="text-sm text-slate-500">Manage your tasks and workflows</p>
        </div>

        {workspace.boards && workspace.boards.length > 0 ? (
          <div className="flex items-start gap-6">
            {workspace.boards[0].columns.sort((a, b) => a.position - b.position).map(column => (
              <div key={column.id} className="w-80 shrink-0 bg-slate-100/80 rounded-xl p-3 border border-slate-200/60">
                <div className="flex items-center justify-between mb-3 px-1">
                  <h3 className="font-semibold text-slate-700 text-sm">{column.name}</h3>
                  <span className="text-xs font-semibold text-slate-400 bg-slate-200/50 px-2 py-0.5 rounded-full">0</span>
                </div>
                <div className="min-h-25 rounded-lg border-2 border-dashed border-slate-200 flex items-center justify-center text-sm font-medium text-slate-400 bg-slate-50/50">
                  Drop tasks here
                </div>
              </div>
            ))}
            <button className="w-80 shrink-0 bg-slate-100/50 hover:bg-slate-100/80 rounded-xl p-3 border border-slate-200/60 transition-colors flex items-center justify-center gap-2 text-slate-500 font-medium text-sm h-12">
              <span className="text-lg leading-none">+</span> Add Column
            </button>
          </div>
        ) : (
          <div className="bg-white p-8 rounded-xl border border-slate-200 shadow-sm text-center">
            <p className="text-slate-500">No boards configured yet.</p>
          </div>
        )}
      </main>
    </div>
  );
}
