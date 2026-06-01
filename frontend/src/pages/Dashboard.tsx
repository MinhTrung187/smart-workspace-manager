import { useState } from 'react';
import { useNavigate } from 'react-router';
import { LayoutTemplate, LogOut, Plus } from 'lucide-react';
import WorkspaceList from '../features/workspace/components/WorkspaceList';
import CreateWorkspaceModal from '../features/workspace/components/CreateWorkspaceModal';
import InvitationsList from '../features/workspace/components/InvitationsList';

export default function Dashboard() {
  const navigate = useNavigate();
  const [isModalOpen, setIsModalOpen] = useState(false);

  const handleLogout = () => {
    localStorage.removeItem('accessToken');
    navigate('/login');
  };

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="bg-white border-b border-slate-200 sticky top-0 z-10">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16 items-center">
            <div className="flex items-center gap-2">
                <div className="p-1.5 bg-indigo-600 rounded-lg">
                    <LayoutTemplate className="w-5 h-5 text-white" />
                </div>
                <span className="text-lg font-bold text-slate-900 tracking-tight">Smart Workspace</span>
            </div>
            <button
              onClick={handleLogout}
              className="inline-flex items-center gap-2 px-3 py-2 border border-slate-300 shadow-sm text-sm font-medium rounded-md text-slate-700 bg-white hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-colors"
            >
              <LogOut className="w-4 h-4" />
              Sign out
            </button>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
        <InvitationsList />
        <div className="flex items-center justify-between mb-8">
          <div>
            <h1 className="text-2xl font-bold text-slate-900 tracking-tight">Your Workspaces</h1>
            <p className="text-sm text-slate-500 font-medium mt-1">Select a workspace to view its boards and tasks.</p>
          </div>
          <button
            onClick={() => setIsModalOpen(true)}
            className="inline-flex items-center gap-2 px-4 py-2 border border-transparent shadow-sm text-sm font-semibold rounded-lg text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-all"
          >
            <Plus className="w-4 h-4" />
            New Workspace
          </button>
        </div>

        <WorkspaceList onCreateClick={() => setIsModalOpen(true)} />

        <CreateWorkspaceModal 
          isOpen={isModalOpen} 
          onClose={() => setIsModalOpen(false)} 
        />
      </main>
    </div>
  );
}
