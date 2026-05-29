import { Link, useParams } from 'react-router';
import { ArrowLeft, Calendar, LayoutPanelTop, User, Filter, Share2, MoreHorizontal } from 'lucide-react';
import KanbanBoard from '../features/board/components/KanbanBoard';
import { useBoardDetailQuery } from '../features/board/hooks/useBoard';

export default function BoardDetail() {
  const { workspaceId, boardId } = useParams<{ workspaceId: string; boardId: string }>();
  const { data: board, isLoading, isError } = useBoardDetailQuery(boardId || '');

  const workspacePath = workspaceId ? `/workspaces/${workspaceId}` : '/dashboard';

  if (isLoading) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center">
        <div className="flex justify-center items-center gap-3">
            <div className="w-6 h-6 border-2 border-indigo-600 border-t-transparent rounded-full animate-spin"></div>
            <span className="text-sm font-medium text-slate-600">Loading board...</span>
        </div>
      </div>
    );
  }

  if (isError || !board) {
    return (
      <div className="min-h-screen bg-slate-50 p-8 flex items-center justify-center">
        <div className="max-w-md w-full bg-white p-8 rounded-2xl border border-red-100 shadow-sm text-center">
          <div className="w-16 h-16 bg-red-50 rounded-full flex items-center justify-center mx-auto mb-4">
             <LayoutPanelTop className="w-8 h-8 text-red-500" />
          </div>
          <h2 className="text-lg font-bold text-slate-900 mb-2">Board not found</h2>
          <p className="text-slate-500 mb-6">The board you are looking for doesn't exist or failed to load.</p>
          <Link to={workspacePath} className="inline-flex items-center justify-center px-4 py-2.5 text-sm font-semibold text-white bg-indigo-600 rounded-lg shadow-sm hover:bg-indigo-700 transition-all focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2">
            Return to Workspace
          </Link>
        </div>
      </div>
    );
  }

  const createdAt = new Date(board.createdAt).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });

  return (
    <div className="h-screen overflow-hidden bg-slate-100 flex flex-col font-sans selection:bg-indigo-100 selection:text-indigo-900">
      <header className="shrink-0 bg-white border-b border-slate-300 sticky top-0 z-20 shadow-sm">
        <div className="w-full h-14 px-4 flex items-center justify-between">
          <div className="flex items-center gap-3 overflow-hidden">
             <Link 
               to={workspacePath} 
               className="shrink-0 w-8 h-8 flex items-center justify-center text-slate-400 hover:text-slate-700 hover:bg-slate-100 rounded-lg transition-colors"
             >
               <ArrowLeft className="w-4 h-4" />
             </Link>
             <div className="h-4 w-px bg-slate-300 shrink-0"></div>
             <div className="flex items-center gap-2.5 min-w-0">
                <div className="w-7 h-7 bg-indigo-600 rounded-md shrink-0 flex items-center justify-center shadow-sm shadow-indigo-300/60">
                  <LayoutPanelTop className="w-3.5 h-3.5 text-white" />
                </div>
                <div className="flex items-center gap-1.5 min-w-0 truncate text-sm">
                   <span className="font-medium text-slate-500 shrink-0 hidden sm:inline">Workspace /</span>
                   <span className="font-semibold text-slate-900 truncate">{board.name}</span>
                </div>
             </div>
          </div>
          
          <div className="flex items-center gap-2 shrink-0 ml-4">
             <div className="hidden md:flex items-center gap-4 text-[13px] font-medium text-slate-500 mr-2">
                <div className="flex items-center gap-1.5" title="Owner">
                  <User className="w-3.5 h-3.5 text-slate-400" />
                  <span className="max-w-25 truncate">{board.createdByName || 'Unknown'}</span>
                </div>
                <div className="flex items-center gap-1.5" title="Created At">
                  <Calendar className="w-3.5 h-3.5 text-slate-400" />
                  <span>{createdAt}</span>
                </div>
             </div>
             
             <div className="hidden md:block h-4 w-px bg-slate-300 mx-1"></div>

             <button className="h-8 px-3 inline-flex items-center justify-center gap-1.5 text-sm font-semibold text-indigo-700 bg-indigo-50 border border-indigo-200 hover:bg-indigo-100 hover:text-indigo-900 rounded-lg transition-colors shadow-sm">
               <Share2 className="w-3.5 h-3.5" />
               <span className="hidden sm:inline">Share</span>
             </button>
             <button className="h-8 w-8 inline-flex items-center justify-center text-slate-400 hover:text-slate-600 hover:bg-slate-100 rounded-lg transition-colors">
               <MoreHorizontal className="w-4 h-4" />
             </button>
          </div>
        </div>
      </header>

      <main className="flex-1 flex flex-col min-h-0">
        <div className="shrink-0 bg-white border-b border-slate-300 pb-0.5 shadow-sm">
          <div className="px-4 sm:px-6 py-6 sm:py-8 max-w-450 w-full mx-auto">
            <div className="flex flex-col md:flex-row md:items-end justify-between gap-4">
              <div className="w-full flex-1 min-w-0">
                <div className="flex items-center gap-2 mb-2">
                   <span className="inline-flex items-center px-2 py-0.5 rounded text-[11px] font-bold tracking-wider text-indigo-800 bg-indigo-100 ring-1 ring-inset ring-indigo-300 uppercase">
                     {board.workspaceName || 'Board'}
                   </span>
                </div>
                <h2 className="text-3xl sm:text-4xl font-extrabold text-slate-900 tracking-tight truncate pb-1">
                  {board.name}
                </h2>
              </div>
              <div className="flex items-center shrink-0">
                 <button className="h-9 px-4 inline-flex items-center justify-center gap-2 text-sm font-semibold text-slate-800 bg-white border border-slate-400 hover:border-indigo-400 hover:bg-indigo-50 hover:text-indigo-800 rounded-lg transition-all shadow-sm focus:ring-2 focus:ring-indigo-500 focus:outline-none">
                    <Filter className="w-4 h-4 text-indigo-500" />
                    Filter Tasks
                 </button>
              </div>
            </div>
          </div>
        </div>

        <div className="flex-1 min-h-0 p-4 sm:p-6 w-full max-w-450 mx-auto overflow-hidden flex flex-col">
          <KanbanBoard board={board} />
        </div>
      </main>
    </div>
  );
}
