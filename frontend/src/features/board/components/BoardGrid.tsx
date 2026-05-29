import { useState } from 'react';
import { Link } from 'react-router';
import { Plus, LayoutPanelTop, Calendar, User, ArrowRight } from 'lucide-react';
import CreateBoardModal from './CreateBoardModal';
import type { BoardDto } from '../../workspace/types';

interface BoardGridProps {
  workspaceId: string;
  boards: BoardDto[];
}

export default function BoardGrid({ workspaceId, boards }: BoardGridProps) {
  const [isModalOpen, setIsModalOpen] = useState(false);

  return (
    <>
      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
        
        {/* Create New Board Card */}
        <button
          onClick={() => setIsModalOpen(true)}
          className="group flex flex-col items-center justify-center min-h-40 bg-white border-2 border-dashed border-slate-200 hover:border-indigo-400 hover:bg-indigo-50/50 rounded-xl p-6 text-center shadow-sm transition-all focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
        >
          <div className="w-12 h-12 bg-indigo-50 text-indigo-600 group-hover:bg-indigo-100 rounded-xl flex items-center justify-center mb-4 transition-colors">
            <Plus className="w-6 h-6" />
          </div>
          <h3 className="text-sm font-bold text-slate-900 group-hover:text-indigo-700">Create New Board</h3>
          <p className="text-xs text-slate-500 mt-1">Start a new project workflow</p>
        </button>

        {/* Existing Boards */}
        {boards.map((board) => {
          const dateObj = new Date(board.createdAt);
          const dateStr = dateObj.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });

          return (
            <Link
              key={board.id}
              to={`/workspaces/${workspaceId}/boards/${board.id}`}
              className="group flex flex-col justify-between min-h-40 bg-white border border-slate-200 rounded-xl p-5 shadow-sm hover:shadow-md hover:border-indigo-200 transition-all focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
            >
              <div>
                <div className="flex items-center justify-between mb-3">
                  <div className="p-2 bg-slate-50 border border-slate-100 rounded-lg text-slate-600">
                    <LayoutPanelTop className="w-4 h-4" />
                  </div>
                  <div className="w-7 h-7 rounded-full bg-slate-50 border border-slate-200 flex items-center justify-center text-slate-400 group-hover:text-indigo-600 group-hover:bg-indigo-50 transition-colors">
                    <ArrowRight className="w-3.5 h-3.5" />
                  </div>
                </div>
                <h3 className="text-base font-bold text-slate-900 group-hover:text-indigo-600 transition-colors line-clamp-1 mb-1">
                  {board.name}
                </h3>
              </div>
              
              <div className="flex items-center justify-between pt-3 border-t border-slate-100 text-xs font-medium text-slate-500 mt-2">
                <div className="flex items-center gap-1 min-w-0 pr-2">
                  <User className="w-3.5 h-3.5 shrink-0" />
                  <span className="truncate">{board.createdByName || 'Unknown'}</span>
                </div>
                <div className="flex items-center gap-1 shrink-0">
                  <Calendar className="w-3.5 h-3.5" />
                  <span>{dateStr}</span>
                </div>
              </div>
            </Link>
          );
        })}
      </div>

      <CreateBoardModal 
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        workspaceId={workspaceId}
      />
    </>
  );
}
