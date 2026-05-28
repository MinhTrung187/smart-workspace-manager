import { useNavigate } from 'react-router';
import { LayoutTemplate, LogOut } from 'lucide-react';

export default function Dashboard() {
  const navigate = useNavigate();

  const handleLogout = () => {
    localStorage.removeItem('accessToken');
    navigate('/login');
  };

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="bg-white border-b border-slate-200">
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
        <div className="bg-white p-8 rounded-xl border border-slate-200 shadow-sm text-center">
             <h1 className="text-2xl font-semibold text-slate-900">Welcome to the Dashboard</h1>
             <p className="mt-2 text-slate-500">You have successfully signed in. Project management interface will appear here.</p>
        </div>
      </main>
    </div>
  );
}
