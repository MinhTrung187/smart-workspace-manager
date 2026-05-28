import { LayoutTemplate, CheckCircle2 } from 'lucide-react';
import LoginForm from '../features/auth/component/LoginForm';

export default function Login() {
  return (
    <div className="flex min-h-screen bg-slate-50 text-slate-900 font-sans">
      {/* Decorative Side Panel for SaaS vibe */}
      <div className="hidden lg:flex flex-col justify-between w-1/2 bg-indigo-600 p-12 text-white overflow-hidden relative">
        <div className="absolute top-0 left-0 w-full h-full opacity-20">
          <div className="absolute -top-24 -left-24 w-96 h-96 rounded-full bg-indigo-400 blur-3xl"></div>
          <div className="absolute bottom-12 right-12 w-80 h-80 rounded-full bg-indigo-800 blur-3xl"></div>
        </div>
        
        <div className="relative z-10 flex items-center gap-2">
          <div className="p-2 bg-white/10 rounded-xl backdrop-blur-sm border border-white/20">
            <LayoutTemplate className="w-6 h-6 text-indigo-100" />
          </div>
          <span className="text-xl font-semibold tracking-tight">Smart Workspace</span>
        </div>

        <div className="relative z-10 max-w-md">
          <h1 className="text-4xl font-bold mb-6 leading-tight">Manage your projects seamlessly.</h1>
          <div className="space-y-4 text-indigo-100 mb-12">
            <div className="flex items-center gap-3">
              <CheckCircle2 className="w-5 h-5 text-indigo-300" />
              <span>Visual Kanban task tracking</span>
            </div>
            <div className="flex items-center gap-3">
              <CheckCircle2 className="w-5 h-5 text-indigo-300" />
              <span>Real-time team collaboration</span>
            </div>
            <div className="flex items-center gap-3">
              <CheckCircle2 className="w-5 h-5 text-indigo-300" />
              <span>Minimalist, productivity-focused UI</span>
            </div>
          </div>
        </div>

        <div className="relative z-10 text-sm text-indigo-200">
          © {new Date().getFullYear()} Smart Workspace Inc. All rights reserved.
        </div>
      </div>

      {/* Main Login Form */}
      <div className="flex flex-col justify-center w-full lg:w-1/2 p-8 sm:p-12 lg:p-24 relative overflow-y-auto">
        <div className="w-full max-w-sm mx-auto mb-8 lg:hidden">
          {/* Logo for Mobile */}
          <div className="flex items-center gap-2">
             <div className="p-2 bg-indigo-100 rounded-xl border border-indigo-200">
                <LayoutTemplate className="w-6 h-6 text-indigo-600" />
             </div>
             <span className="text-xl font-bold text-slate-800 tracking-tight">Smart Workspace</span>
          </div>
        </div>

        <LoginForm />
      </div>
    </div>
  );
}
