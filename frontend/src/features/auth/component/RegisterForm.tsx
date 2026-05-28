import { useState } from 'react';
import type { FormEvent } from 'react';
import { Eye, EyeOff, Mail, Lock, User } from 'lucide-react';
import { useRegisterMutation } from '../hooks/useAuthMutations';

export default function RegisterForm() {
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    password: '',
    confirmPassword: '',
  });

  const [fieldErrors, setFieldErrors] = useState<Partial<typeof formData>>({});
  const [apiError, setApiError] = useState<string | null>(null);

  const mutation = useRegisterMutation();

  const validate = () => {
    const errors: Partial<typeof formData> = {};
    if (!formData.fullName.trim()) {
      errors.fullName = 'Please enter your full name';
    }
    
    if (!formData.email) {
      errors.email = 'Please enter your email';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      errors.email = 'Invalid email address';
    }
    
    if (!formData.password) {
      errors.password = 'Please enter a password';
    } else if (formData.password.length < 6) {
      errors.password = 'Password must be at least 6 characters';
    }

    if (formData.password !== formData.confirmPassword) {
      errors.confirmPassword = 'Passwords do not match';
    }

    setFieldErrors({ ...errors });
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    setApiError(null);
    if (validate()) {
      mutation.mutate({
        fullName: formData.fullName,
        email: formData.email,
        password: formData.password
      }, {
        onError: (error: any) => {
          const message = error.response?.data?.message || 'Unable to create account. Please try again later.';
          setApiError(message);
        }
      });
    }
  };

  return (
    <div className="bg-white px-8 py-10 shadow-sm border border-slate-200 rounded-2xl">
      <form onSubmit={handleSubmit} className="space-y-5">
        {apiError && (
          <div className="p-3 bg-red-50 border border-red-200 rounded-lg flex items-center gap-3 text-red-600 text-sm font-medium animate-in fade-in zoom-in-95">
            <div className="w-1.5 h-1.5 rounded-full bg-red-600 shrink-0"></div>
            {apiError}
          </div>
        )}

        <div>
          <label className="block text-sm font-semibold text-slate-700 mb-1.5">Full Name</label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <User className="h-5 w-5 text-slate-400" />
            </div>
            <input
              type="text"
              placeholder="John Doe"
              className={`block w-full pl-10 pr-3 py-2.5 text-slate-900 bg-slate-50/50 border ${fieldErrors.fullName ? 'border-red-400 focus:ring-red-500 bg-red-50/10' : 'border-slate-300 focus:border-indigo-500 focus:ring-indigo-500'} rounded-lg shadow-sm placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:bg-white transition-all sm:text-sm`}
              value={formData.fullName}
              onChange={(e) => setFormData({ ...formData, fullName: e.target.value })}
            />
          </div>
          {fieldErrors.fullName && <p className="mt-1.5 text-sm text-red-500 font-medium">{fieldErrors.fullName}</p>}
        </div>

        <div>
          <label className="block text-sm font-semibold text-slate-700 mb-1.5">Work Email</label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <Mail className="h-5 w-5 text-slate-400" />
            </div>
            <input
              type="email"
              placeholder="name@company.com"
              className={`block w-full pl-10 pr-3 py-2.5 text-slate-900 bg-slate-50/50 border ${fieldErrors.email ? 'border-red-400 focus:ring-red-500 bg-red-50/10' : 'border-slate-300 focus:border-indigo-500 focus:ring-indigo-500'} rounded-lg shadow-sm placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:bg-white transition-all sm:text-sm`}
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            />
          </div>
          {fieldErrors.email && <p className="mt-1.5 text-sm text-red-500 font-medium">{fieldErrors.email}</p>}
        </div>

        <div>
          <label className="block text-sm font-semibold text-slate-700 mb-1.5">Password</label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <Lock className="h-5 w-5 text-slate-400" />
            </div>
            <input
              type={showPassword ? "text" : "password"}
              placeholder="Minimum 6 characters"
              className={`block w-full pl-10 pr-10 py-2.5 text-slate-900 bg-slate-50/50 border ${fieldErrors.password ? 'border-red-400 focus:ring-red-500 bg-red-50/10' : 'border-slate-300 focus:border-indigo-500 focus:ring-indigo-500'} rounded-lg shadow-sm placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:bg-white transition-all sm:text-sm`}
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

        <div>
          <label className="block text-sm font-semibold text-slate-700 mb-1.5">Confirm Password</label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <Lock className="h-5 w-5 text-slate-400" />
            </div>
            <input
              type={showConfirmPassword ? "text" : "password"}
              placeholder="Confirm your password"
              className={`block w-full pl-10 pr-10 py-2.5 text-slate-900 bg-slate-50/50 border ${fieldErrors.confirmPassword ? 'border-red-400 focus:ring-red-500 bg-red-50/10' : 'border-slate-300 focus:border-indigo-500 focus:ring-indigo-500'} rounded-lg shadow-sm placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:bg-white transition-all sm:text-sm`}
              value={formData.confirmPassword}
              onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
            />
             <button
              type="button"
              onClick={() => setShowConfirmPassword(!showConfirmPassword)}
              className="absolute inset-y-0 right-0 pr-3 flex items-center text-slate-400 hover:text-slate-600 focus:outline-none"
            >
              {showConfirmPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
            </button>
          </div>
          {fieldErrors.confirmPassword && <p className="mt-1.5 text-sm text-red-500 font-medium">{fieldErrors.confirmPassword}</p>}
        </div>

        <div className="pt-2">
           <button
             type="submit"
             disabled={mutation.isPending}
             className="w-full flex justify-center py-2.5 px-4 border border-transparent rounded-lg shadow-sm text-sm font-semibold text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-70 disabled:cursor-not-allowed transition-all"
           >
             {mutation.isPending ? (
               <div className="flex items-center gap-2">
                 <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                   <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                   <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                 </svg>
                 Processing...
               </div>
             ) : (
               'Create account'
             )}
           </button>
        </div>
      </form>
    </div>
  );
}
