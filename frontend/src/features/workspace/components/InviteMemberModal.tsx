import { useState, useEffect } from 'react';
import type { FormEvent } from 'react';
import { useSendInvitationMutation } from '../hooks/useWorkspaceInvitationMutations';
import { X, Loader2, MailCheck, Send } from 'lucide-react';

interface InviteMemberModalProps {
  isOpen: boolean;
  onClose: () => void;
  workspaceId: string;
  workspaceName: string;
}

export default function InviteMemberModal({ isOpen, onClose, workspaceId, workspaceName }: InviteMemberModalProps) {
  const [email, setEmail] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSuccess, setIsSuccess] = useState(false);
  const [sentLink, setSentLink] = useState<string | null>(null);

  const mutation = useSendInvitationMutation(workspaceId);

  useEffect(() => {
    if (isOpen) {
      setEmail('');
      setError(null);
      setIsSuccess(false);
      setSentLink(null);
    }
  }, [isOpen]);

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    setError(null);

    const emailTrimmed = email.trim();
    if (!emailTrimmed) {
      setError('Email address is required');
      return;
    }

    // Basic email format validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(emailTrimmed)) {
      setError('Please enter a valid email address');
      return;
    }

    mutation.mutate(emailTrimmed, {
      onError: (err: any) => {
        setError(err.response?.data?.message || 'Failed to send invitation. Please try again.');
      },
      onSuccess: (data) => {
        setIsSuccess(true);
        if (data.inviteLink) {
          setSentLink(data.inviteLink);
        }
        // Auto-close modal after 3 seconds or let user dismiss
        const timer = setTimeout(() => {
          onClose();
        }, 3500);
        return () => clearTimeout(timer);
      }
    });
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm animate-in fade-in duration-200">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md overflow-hidden animate-in zoom-in-95 duration-200">
        
        {/* Header */}
        <div className="flex justify-between items-center p-6 border-b border-slate-100">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-indigo-50 text-indigo-600 rounded-lg">
              <Send className="w-5 h-5" />
            </div>
            <h2 className="text-xl font-semibold text-slate-900">Invite Member</h2>
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

        {/* Content */}
        <div className="p-6">
          {isSuccess ? (
            <div className="text-center py-6 space-y-4">
              <div className="mx-auto h-12 w-12 bg-emerald-100 text-emerald-600 rounded-full flex items-center justify-center">
                <MailCheck className="w-6 h-6 animate-bounce" />
              </div>
              <div>
                <h3 className="text-lg font-bold text-slate-900">Invitation Sent Successfully!</h3>
                <p className="text-sm text-slate-500 mt-1 max-w-xs mx-auto">
                  An email invitation has been generated for <span className="font-semibold text-slate-700">{email}</span> to join <span className="font-semibold text-indigo-600">{workspaceName}</span>.
                </p>
              </div>

              {sentLink && (
                <div className="mt-4 p-3 bg-slate-50 rounded-xl border border-slate-200 text-left">
                  <span className="block text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">Direct Invite Link</span>
                  <input
                    type="text"
                    readOnly
                    value={sentLink}
                    onClick={(e) => (e.target as HTMLInputElement).select()}
                    className="w-full bg-white border border-slate-200 rounded-lg px-2.5 py-1.5 text-xs text-indigo-600 font-mono shadow-sm focus:outline-none cursor-pointer"
                  />
                  <span className="block text-[9px] text-slate-500 mt-1">Click input to select and copy link for direct registration as fallback.</span>
                </div>
              )}

              <div className="pt-2">
                <button
                  type="button"
                  onClick={onClose}
                  className="w-full py-2.5 text-sm font-semibold text-white bg-indigo-600 hover:bg-indigo-700 rounded-lg shadow-sm transition-colors"
                >
                  Done
                </button>
              </div>
            </div>
          ) : (
            <form onSubmit={handleSubmit} className="space-y-5">
              <p className="text-sm text-slate-500 leading-relaxed">
                Invite colleagues or team members to collaborate on <span className="font-bold text-slate-800">{workspaceName}</span> boards. We'll send them an active invitation.
              </p>

              {error && (
                <div className="p-3 bg-red-50 border border-red-100 rounded-lg flex items-center gap-2 text-red-600 text-sm font-medium">
                  <div className="w-1.5 h-1.5 rounded-full bg-red-600 shrink-0"></div>
                  {error}
                </div>
              )}

              <div>
                <label htmlFor="member-email" className="block text-sm font-semibold text-slate-700 mb-1.5">
                  Email Address <span className="text-red-500">*</span>
                </label>
                <input
                  id="member-email"
                  type="email"
                  placeholder="name@company.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  disabled={mutation.isPending}
                  required
                  className="block w-full px-3 py-2.5 text-slate-900 bg-white border border-slate-300 rounded-lg shadow-sm placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-shadow sm:text-sm disabled:bg-slate-50 disabled:text-slate-500"
                  autoFocus
                />
              </div>

              <div className="flex gap-3 justify-end pt-2">
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
                      Sending...
                    </>
                  ) : (
                    'Send Invitation'
                  )}
                </button>
              </div>
            </form>
          )}
        </div>
      </div>
    </div>
  );
}
