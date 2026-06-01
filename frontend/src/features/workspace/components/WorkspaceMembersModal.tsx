import { useState, useRef, useEffect } from 'react';
import { useWorkspaceMembersQuery } from '../hooks/useWorkspace';
import { X, Loader2, MoreVertical, Shield, User, Trash2, ShieldAlert } from 'lucide-react';

interface WorkspaceMembersModalProps {
  isOpen: boolean;
  onClose: () => void;
  workspaceId: string;
}

// Simple client-side JWT decoder to inspect the current logged-in user without external libraries
const getCurrentUserEmail = (): string | null => {
  const token = localStorage.getItem('accessToken');
  if (!token) return null;
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      window
        .atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    const parsed = JSON.parse(jsonPayload);
    // Standard JWT claims check
    return parsed.email || parsed.unique_name || parsed.sub || null;
  } catch (e) {
    return null;
  }
};

export default function WorkspaceMembersModal({ isOpen, onClose, workspaceId }: WorkspaceMembersModalProps) {
  const { data: members, isLoading, isError, refetch } = useWorkspaceMembersQuery(workspaceId);
  const [activeMenuMemberId, setActiveMenuMemberId] = useState<string | null>(null);
  const currentEmail = getCurrentUserEmail();
  const menuRef = useRef<HTMLDivElement | null>(null);

  // Close the popup dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setActiveMenuMemberId(null);
      }
    };

    if (activeMenuMemberId) {
      document.addEventListener('mousedown', handleClickOutside);
    }
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [activeMenuMemberId]);

  if (!isOpen) return null;

  const handleActionStub = (actionType: 'role' | 'kick', memberId: string, memberName: string) => {
    console.log(`[Workspace Management Stub] Action: ${actionType} performed on member: ${memberName} (${memberId})`);
    alert(`Action executed: '${actionType === 'role' ? 'Change Role' : 'Kick Member'}' for ${memberName}. (Backend integration is currently stubbed).`);
    setActiveMenuMemberId(null);
  };

  const getRoleBadgeStyle = (role: string) => {
    switch (role?.toLowerCase()) {
      case 'owner':
        return 'bg-amber-50 text-amber-700 border-amber-200/60';
      case 'admin':
        return 'bg-purple-50 text-purple-700 border-purple-200/60';
      default:
        return 'bg-slate-50 text-slate-600 border-slate-200/60';
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm animate-in fade-in duration-200">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg overflow-hidden animate-in zoom-in-95 duration-200 flex flex-col max-h-[85vh]">
        
        {/* Header */}
        <div className="flex justify-between items-center p-6 border-b border-slate-100 shrink-0">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-indigo-50 text-indigo-600 rounded-lg">
              <Shield className="w-5 h-5" />
            </div>
            <div>
              <h2 className="text-xl font-bold text-slate-900 leading-none">Workspace Members</h2>
              <p className="text-xs text-slate-500 mt-1 font-medium">View and manage collaborator access permissions.</p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="text-slate-400 hover:text-slate-600 transition-colors"
            aria-label="Close modal"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content Container */}
        <div className="p-6 overflow-y-auto flex-1">
          {isLoading ? (
            <div className="flex flex-col items-center justify-center py-12 gap-3">
              <Loader2 className="w-8 h-8 text-indigo-600 animate-spin" />
              <span className="text-sm text-slate-500 font-semibold animate-pulse">Loading workspace members...</span>
            </div>
          ) : isError ? (
            <div className="text-center py-10 space-y-3">
              <p className="text-sm text-red-600 font-bold">Failed to load workspace members list.</p>
              <button
                onClick={() => refetch()}
                className="px-4 py-2 text-xs font-semibold text-white bg-indigo-600 hover:bg-indigo-700 rounded-lg shadow-sm"
              >
                Reload Members
              </button>
            </div>
          ) : members && members.length === 0 ? (
            <div className="text-center py-12">
              <div className="mx-auto h-12 w-12 text-slate-300 flex items-center justify-center bg-slate-50 rounded-full mb-3">
                <User className="w-6 h-6" />
              </div>
              <h3 className="text-sm font-bold text-slate-700">No Members</h3>
              <p className="text-xs text-slate-400 mt-1">This workspace doesn't have any registered members.</p>
            </div>
          ) : (
            <div className="divide-y divide-slate-100">
              {members?.map((member) => {
                const initials = member.fullName
                  ? member.fullName.split(' ').map((n) => n[0]).join('').slice(0, 2).toUpperCase()
                  : '?';
                
                const isCurrentUser = member.email === currentEmail;
                const isMenuOpen = activeMenuMemberId === member.userId;

                return (
                  <div
                    key={member.userId}
                    className="flex items-center justify-between py-4 first:pt-1 last:pb-1 group/row"
                  >
                    {/* User Profile Info */}
                    <div className="flex items-center gap-3.5 min-w-0">
                      <div className="relative shrink-0 h-10 w-10 rounded-full bg-slate-100 border border-slate-250 flex items-center justify-center text-xs font-bold text-slate-700 shadow-inner">
                        {member.avatarUrl ? (
                          <img
                            src={member.avatarUrl}
                            alt={member.fullName}
                            className="h-full w-full rounded-full object-cover"
                            referrerPolicy="no-referrer"
                          />
                        ) : (
                          <span>{initials}</span>
                        )}
                        {isCurrentUser && (
                          <div className="absolute -bottom-0.5 -right-0.5 h-3 w-3 rounded-full bg-emerald-500 border border-white" title="Active Account" />
                        )}
                      </div>

                      <div className="flex flex-col min-w-0">
                        <div className="flex items-center gap-2">
                          <span className="text-sm font-bold text-slate-800 truncate leading-none">
                            {member.fullName}
                          </span>
                          {isCurrentUser && (
                            <span className="inline-flex items-center px-1.5 py-0.5 text-[9px] font-bold text-emerald-700 bg-emerald-50 border border-emerald-200/50 rounded-md">
                              You
                            </span>
                          )}
                        </div>
                        <span className="text-xs text-neutral-500 mt-0.5 truncate">
                          {member.email}
                        </span>
                      </div>
                    </div>

                    {/* Role Tag & Options Button */}
                    <div className="flex items-center gap-3 shrink-0 relative">
                      <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold border ${getRoleBadgeStyle(member.role)}`}>
                        {member.role || 'Member'}
                      </span>

                      {!isCurrentUser && (
                        <div>
                          <button
                            onClick={() => setActiveMenuMemberId(isMenuOpen ? null : member.userId)}
                            className="p-1.5 text-slate-400 hover:text-slate-600 hover:bg-slate-100 transition-colors rounded-lg focus:outline-none"
                            aria-label="Actions menu"
                          >
                            <MoreVertical className="w-4 h-4" />
                          </button>

                          {/* Dropdown Options Container */}
                          {isMenuOpen && (
                            <div
                              ref={menuRef}
                              className="absolute right-0 mt-1 w-44 bg-white border border-slate-200 rounded-xl shadow-lg z-30 py-1.5 overflow-hidden animate-in fade-in slide-in-from-top-1 duration-150"
                            >
                              <button
                                onClick={() => handleActionStub('role', member.userId, member.fullName)}
                                className="w-full text-left px-3.5 py-2 text-xs font-semibold text-slate-700 hover:bg-slate-50 transition-colors flex items-center gap-2"
                              >
                                <ShieldAlert className="w-4 h-4 text-purple-600" />
                                Change Role
                              </button>
                              <button
                                onClick={() => handleActionStub('kick', member.userId, member.fullName)}
                                className="w-full text-left px-3.5 py-2 text-xs font-semibold text-red-600 hover:bg-red-50 transition-colors flex items-center gap-2"
                              >
                                <Trash2 className="w-4 h-4" />
                                Kick Member
                              </button>
                            </div>
                          )}
                        </div>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="p-4 border-t border-slate-100 bg-slate-50/50 flex justify-end shrink-0">
          <button
            onClick={onClose}
            className="px-4 py-2.5 text-xs font-semibold text-slate-700 bg-white border border-slate-300 rounded-lg shadow-sm hover:bg-slate-50 focus:outline-none transition-colors"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  );
}
