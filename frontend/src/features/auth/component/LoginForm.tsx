import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link } from 'react-router';
import { Eye, EyeOff, Mail, Lock } from 'lucide-react';
import { useLoginMutation } from '../hooks/useAuthMutations';

export default function LoginForm() {
  const [showPassword, setShowPassword] = useState(false);
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    rememberMe: false,
  });

  const [fieldErrors, setFieldErrors] = useState<{ email?: string; password?: string }>({});
  const [apiError, setApiError] = useState<string | null>(null);

  const mutation = useLoginMutation();

  const validate = () => {
    const errors: { email?: string; password?: string } = {};
    if (!formData.email) {
      errors.email = 'Please enter your email';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      errors.email = 'Invalid email address';
    }
    
    if (!formData.password) {
      errors.password = 'Please enter your password';
    }

    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    setApiError(null);
    if (validate()) {
      mutation.mutate(formData, {
        onError: (error: any) => {
          const message = error.response?.data?.message || 'Invalid email or password. Please try again.';
          setApiError(message);
        }
      });
    }
  };

  return (
    <div className="w-full max-w-sm mx-auto">
      <div className="mb-8 pl-1">
        <h2 className="text-2xl font-bold text-slate-900 tracking-tight mb-2">Sign in to your account</h2>
        <p className="text-sm text-slate-500 font-medium">Welcome back! Please enter your details.</p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-5">
        {apiError && (
          <div className="p-3 bg-red-50 border border-red-200 rounded-lg flex items-center gap-3 text-red-600 text-sm font-medium animate-in fade-in slide-in-from-top-1">
            <div className="w-1.5 h-1.5 rounded-full bg-red-600 shrink-0"></div>
            {apiError}
          </div>
        )}

        <div>
          <label className="block text-sm font-semibold text-slate-700 mb-1.5" htmlFor="email">
            Email
          </label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <Mail className="h-5 w-5 text-slate-400" />
            </div>
            <input
              id="email"
              type="email"
              placeholder="name@company.com"
              className={`block w-full pl-10 pr-3 py-2.5 text-slate-900 bg-white border ${fieldErrors.email ? 'border-red-400 focus:ring-red-500' : 'border-slate-300 focus:border-indigo-500 focus:ring-indigo-500'} rounded-lg shadow-sm placeholder:text-slate-400 focus:outline-none focus:ring-2 transition-shadow sm:text-sm`}
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            />
          </div>
          {fieldErrors.email && <p className="mt-1.5 text-sm text-red-500 font-medium">{fieldErrors.email}</p>}
        </div>

        <div>
          <label className="block text-sm font-semibold text-slate-700 mb-1.5" htmlFor="password">
            Password
          </label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <Lock className="h-5 w-5 text-slate-400" />
            </div>
            <input
              id="password"
              type={showPassword ? "text" : "password"}
              placeholder="••••••••"
              className={`block w-full pl-10 pr-10 py-2.5 text-slate-900 bg-white border ${fieldErrors.password ? 'border-red-400 focus:ring-red-500' : 'border-slate-300 focus:border-indigo-500 focus:ring-indigo-500'} rounded-lg shadow-sm placeholder:text-slate-400 focus:outline-none focus:ring-2 transition-shadow sm:text-sm`}
              value={formData.password}
              onChange={(e) => setFormData({ ...formData, password: e.target.value })}
            />
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="absolute inset-y-0 right-0 pr-3 flex items-center text-slate-400 hover:text-slate-600 focus:outline-none"
            >
              {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
            </button>
          </div>
          {fieldErrors.password && <p className="mt-1.5 text-sm text-red-500 font-medium">{fieldErrors.password}</p>}
        </div>

        <div className="flex items-center justify-between mt-4">
          <div className="flex items-center">
            <input
              id="remember-me"
              type="checkbox"
              className="h-4 w-4 rounded border-slate-300 text-indigo-600 focus:ring-indigo-500"
              checked={formData.rememberMe}
              onChange={(e) => setFormData({ ...formData, rememberMe: e.target.checked })}
            />
            <label htmlFor="remember-me" className="ml-2 block text-sm text-slate-700 select-none">
              Remember me
            </label>
          </div>
          <div className="text-sm">
            <a href="#" className="font-semibold text-indigo-600 hover:text-indigo-500 transition-colors">
              Forgot password?
            </a>
          </div>
        </div>

        <button
          type="submit"
          disabled={mutation.isPending}
          className="w-full flex justify-center py-2.5 px-4 border border-transparent rounded-lg shadow-sm text-sm font-semibold text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-70 disabled:cursor-not-allowed transition-all mt-2"
        >
          {mutation.isPending ? (
            <div className="flex items-center gap-2">
              <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              Signing in...
            </div>
          ) : (
            'Sign in'
          )}
        </button>
      </form>

      <p className="mt-8 text-center text-sm text-slate-600 font-medium">
        Don't have an account?{' '}
        <Link to="/register" className="font-semibold text-indigo-600 hover:text-indigo-500 transition-colors">
          Sign up now
        </Link>
      </p>
    </div>
  );
}
