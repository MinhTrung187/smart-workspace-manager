import { useState, useEffect } from 'react';
import type { FormEvent } from 'react';
import { useCreateBoardMutation } from '../hooks/useBoardMutations';
import { X, Loader2, LayoutPanelTop } from 'lucide-react';

interface CreateBoardModalProps {
  isOpen: boolean;
  onClose: () => void;
  workspaceId: string;
}

export default function CreateBoardModal({ isOpen, onClose, workspaceId }: CreateBoardModalProps) {
  const [name, setName] = useState('');
  const [error, setError] = useState<string | null>(null);

  const mutation = useCreateBoardMutation(workspaceId);

  useEffect(() => {
    if (isOpen) {
      setName('');
      setError(null);
    }
  }, [isOpen]);

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!name.trim()) {
      setError('Board name is required');
      return;
    }

    mutation.mutate(
      { workspaceId, name: name.trim() },
      {
        onError: (err: any) => {
          setError(err.response?.data?.message || 'Failed to create board. Please try again.');
        },
        onSuccess: () => {
          onClose(); // Close modal immediately upon success
        }
      }
    );
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm animate-in fade-in duration-200">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md overflow-hidden animate-in zoom-in-95 duration-200">
        <div className="flex justify-between items-center p-6 border-b border-slate-100">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-indigo-50 text-indigo-600 rounded-lg">
              <LayoutPanelTop className="w-5 h-5" />
            </div>
            <h2 className="text-xl font-semibold text-slate-900">New Board</h2>
          </div>
          <button
            onClick={onClose}
            disabled={mutation.isPending}
            className="text-slate-400 hover:text-slate-600 transition-colors disabled:opacity-50"
            aria-label="Close modal"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-5">
          {error && (
            <div className="p-3 bg-red-50 border border-red-100 rounded-lg flex items-center gap-2 text-red-600 text-sm font-medium">
              <div className="w-1.5 h-1.5 rounded-full bg-red-600 shrink-0"></div>
              {error}
            </div>
          )}

          <div>
            <label htmlFor="board-name" className="block text-sm font-semibold text-slate-700 mb-1.5">
              Board Name <span className="text-red-500">*</span>
            </label>
            <input
              id="board-name"
              type="text"
              placeholder="e.g. Q3 Roadmap, Development Sprint"
              value={name}
              onChange={(e) => setName(e.target.value)}
              disabled={mutation.isPending}
              className="block w-full px-3 py-2.5 text-slate-900 bg-white border border-slate-300 rounded-lg shadow-sm placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-shadow sm:text-sm disabled:bg-slate-50 disabled:text-slate-500"
              autoFocus
            />
          </div>

          <div className="pt-2 flex gap-3 justify-end">
            <button
              type="button"
              onClick={onClose}
              disabled={mutation.isPending}
              className="px-4 py-2.5 text-sm font-semibold text-slate-700 bg-white border border-slate-300 rounded-lg shadow-sm hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 transition-colors disabled:opacity-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={mutation.isPending}
              className="inline-flex items-center justify-center min-w-30 px-4 py-2.5 text-sm font-semibold text-white bg-indigo-600 rounded-lg shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 transition-all disabled:opacity-70 disabled:cursor-not-allowed"
            >
              {mutation.isPending ? (
                <>
                  <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                  Creating...
                </>
              ) : (
                'Create Board'
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
