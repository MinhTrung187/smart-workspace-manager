import { Link } from 'react-router';
import { LayoutTemplate } from 'lucide-react';
import RegisterForm from '../features/auth/component/RegisterForm';

export default function Register() {
  return (
    <div className="flex min-h-screen bg-slate-50 text-slate-900 font-sans">
       <div className="flex flex-col justify-center w-full max-w-md mx-auto p-6 sm:p-8">
         <div className="mb-8 text-center">
            <div className="inline-flex items-center gap-2 mb-6">
                <div className="p-2 bg-indigo-600 rounded-xl shadow-sm border border-indigo-700">
                    <LayoutTemplate className="w-5 h-5 text-white" />
                </div>
                <span className="text-xl font-bold tracking-tight text-slate-900">Smart Workspace</span>
            </div>
            <h2 className="text-2xl font-bold text-slate-900 tracking-tight">Create a new account</h2>
            <p className="mt-2 text-sm text-slate-500 font-medium">Start managing high-performance teams</p>
         </div>

         <RegisterForm />

         <p className="mt-8 text-center text-sm text-slate-600 font-medium">
            Already have an account?{' '}
            <Link to="/login" className="font-semibold text-indigo-600 hover:text-indigo-500 transition-colors">
              Sign in now
            </Link>
          </p>
       </div>
    </div>
  );
}
