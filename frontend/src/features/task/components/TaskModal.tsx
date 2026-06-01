import { useState, useEffect } from 'react';
import type { FormEvent } from 'react';
import { X, Calendar, Flag, Loader2, Trash2, UserPlus, Check, ChevronDown } from 'lucide-react';
import type { TaskDto } from '../types';
import { useCreateTask, useUpdateTask, useDeleteTask, useTaskAssigneesQuery, useAssignUserMutation, useUnassignUserMutation } from '../hooks/useTaskMutations';
import { useBoardDetailQuery } from '../../board/hooks/useBoard';
import { useWorkspaceDetailQuery } from '../../workspace/hooks/useWorkspace';

interface TaskModalProps {
  isOpen: boolean;
  onClose: () => void;
  boardId: string;
  mode: 'create' | 'edit';
  columnId: string;
  task?: TaskDto | null;
}

export default function TaskModal({ isOpen, onClose, boardId, mode, columnId, task }: TaskModalProps) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [dueDate, setDueDate] = useState('');
  const [priority, setPriority] = useState<'Low' | 'Medium' | 'High'>('Medium');
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  
  const createTaskMutation = useCreateTask(boardId);
  const updateTaskMutation = useUpdateTask(boardId);
  const deleteTaskMutation = useDeleteTask(boardId);

  // Fetch current task assignees if editing
  const { data: currentAssignees, isLoading: assigneesLoading } = useTaskAssigneesQuery(
    mode === 'edit' && task ? task.id : ''
  );

  // Fetch board details to get workspaceId
  const { data: boardData } = useBoardDetailQuery(boardId);
  const workspaceId = boardData?.workspaceId || '';

  // Fetch workspace details to get workspace members
  const { data: workspaceData, isLoading: workspaceLoading } = useWorkspaceDetailQuery(workspaceId);
  const availableMembers = workspaceData?.members || [];

  const assignMutation = useAssignUserMutation(boardId, task?.id || '');
  const unassignMutation = useUnassignUserMutation(boardId, task?.id || '');

  const getMinDatetimeLocal = () => {
    const dateObj = new Date();
    const offset = dateObj.getTimezoneOffset() * 60000;
    return (new Date(dateObj.getTime() - offset)).toISOString().slice(0, 16);
  };

  const formatToDatetimeLocal = (isoString?: string | null) => {
    if (!isoString) return '';
    const dateObj = new Date(isoString);
    if (isNaN(dateObj.getTime())) return '';
    
    const offset = dateObj.getTimezoneOffset() * 60000;
    return (new Date(dateObj.getTime() - offset)).toISOString().slice(0, 16);
  };

  const handleToggleAssignee = (userId: string, isAssigned: boolean) => {
    if (isAssigned) {
      unassignMutation.mutate(userId);
    } else {
      assignMutation.mutate(userId);
    }
  };

  useEffect(() => {
    if (isOpen) {
      setIsDropdownOpen(false);
      if (mode === 'edit' && task) {
        setTitle(task.title);
        setDescription(task.description || '');
        setPriority(task.priority || 'Medium');
        setDueDate(formatToDatetimeLocal(task.dueDate));
      } else {
        setTitle('');
        setDescription('');
        setDueDate('');
        setPriority('Medium');
      }
    }
  }, [isOpen, mode, task]);

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (!title.trim()) return;

    let isoDueDate: string | null = null;
    if (dueDate) {
      isoDueDate = new Date(dueDate).toISOString();
    }

    if (mode === 'create') {
      createTaskMutation.mutate(
        {
          columnId,
          title: title.trim(),
          description: description.trim(),
          dueDate: isoDueDate,
          priority,
        },
        {
          onSuccess: () => onClose(),
        }
      );
    } else if (mode === 'edit' && task) {
      updateTaskMutation.mutate(
        {
          taskId: task.id,
          data: {
            title: title.trim(),
            description: description.trim(),
            dueDate: isoDueDate,
            priority,
            position: task.position,
            columnId: task.columnId,
          },
        },
        {
          onSuccess: () => onClose(),
        }
      );
    }
  };

  const handleDelete = () => {
    if (window.confirm('Are you sure you want to delete this task?')) {
      if (task) {
        deleteTaskMutation.mutate(task.id, {
          onSuccess: () => onClose(),
        });
      }
    }
  };

  const isPending = createTaskMutation.isPending || updateTaskMutation.isPending || deleteTaskMutation.isPending;

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm animate-in fade-in duration-200">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg overflow-hidden animate-in zoom-in-95 duration-200 border border-slate-200">
        <div className="flex justify-between items-center p-5 border-b border-slate-100 bg-slate-50/50">
          <h2 className="text-lg font-bold text-slate-800">
            {mode === 'create' ? 'Create Task' : 'Edit Task'}
          </h2>
          <button
            type="button"
            onClick={onClose}
            disabled={isPending}
            className="text-slate-400 hover:text-slate-600 transition-colors disabled:opacity-50 p-1 hover:bg-slate-200/50 rounded-md"
            aria-label="Close modal"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-5">
          <div>
            <label htmlFor="task-title" className="block text-sm font-semibold text-slate-700 mb-1.5">
              Title <span className="text-red-500">*</span>
            </label>
            <input
              id="task-title"
              type="text"
              placeholder="What needs to be done?"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              disabled={isPending}
              autoFocus
              className="block w-full px-3 py-2 text-slate-900 bg-white border border-slate-300 rounded-lg shadow-sm placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/30 focus:border-indigo-500 transition-shadow sm:text-sm disabled:bg-slate-50 disabled:text-slate-500"
            />
          </div>

          <div>
            <label htmlFor="task-desc" className="block text-sm font-semibold text-slate-700 mb-1.5">
              Description
            </label>
            <textarea
              id="task-desc"
              rows={4}
              placeholder="Add more details about this task..."
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              disabled={isPending}
              className="block w-full px-3 py-2 text-slate-900 bg-white border border-slate-300 rounded-lg shadow-sm placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/30 focus:border-indigo-500 transition-shadow sm:text-sm disabled:bg-slate-50 disabled:text-slate-500 resize-none"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label htmlFor="task-priority" className="block text-sm font-semibold text-slate-700 mb-1.5 items-center gap-1.5">
                <Flag className="w-4 h-4 text-slate-400" /> Priority
              </label>
              <select
                id="task-priority"
                value={priority}
                onChange={(e) => setPriority(e.target.value as any)}
                disabled={isPending}
                className="block w-full px-3 py-2 text-sm text-slate-900 bg-white border border-slate-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/30 focus:border-indigo-500 transition-shadow disabled:bg-slate-50 disabled:opacity-70"
              >
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
              </select>
            </div>
            
            <div>
              <label htmlFor="task-duedate" className="block text-sm font-semibold text-slate-700 mb-1.5 items-center gap-1.5">
                <Calendar className="w-4 h-4 text-slate-400" /> Due Date
              </label>
              <input
                id="task-duedate"
                type="datetime-local"
                min={getMinDatetimeLocal()}
                value={dueDate}
                onChange={(e) => setDueDate(e.target.value)}
                disabled={isPending}
                className="block w-full px-3 py-2 text-sm text-slate-900 bg-white border border-slate-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/30 focus:border-indigo-500 transition-shadow disabled:bg-slate-50 disabled:opacity-70"
              />
            </div>
          </div>

          {/* Assignees Section */}
          {mode === 'edit' && task && (
            <div className="border-t border-slate-100 pt-4">
              <label className="block text-xs font-bold uppercase tracking-wider text-slate-500 mb-2 items-center gap-1.5">
                <UserPlus className="w-3.5 h-3.5" /> Task Assignees
              </label>
              
              <div className="flex flex-wrap items-center gap-3">
                {/* Stacked list of current assignees */}
                <div className="flex -space-x-2 overflow-hidden">
                  {assigneesLoading ? (
                    <div className="h-8 w-8 rounded-full bg-slate-50 border border-white flex items-center justify-center animate-pulse">
                      <Loader2 className="w-3.5 h-3.5 text-indigo-500 animate-spin" />
                    </div>
                  ) : currentAssignees && currentAssignees.length > 0 ? (
                    currentAssignees.map((assignee) => {
                      const initials = assignee.fullName
                        ? assignee.fullName.split(' ').map((n) => n[0]).join('').slice(0, 2).toUpperCase()
                        : '?';
                      return (
                        <div
                          key={assignee.id}
                          title={assignee.fullName}
                          className="relative group h-8 w-8 rounded-full bg-indigo-500 border-2 border-white flex items-center justify-center text-[10px] font-bold text-white shadow-sm"
                        >
                          {assignee.avatarUrl ? (
                            <img
                              src={assignee.avatarUrl}
                              alt={assignee.fullName}
                              className="h-full w-full rounded-full object-cover"
                              referrerPolicy="no-referrer"
                            />
                          ) : (
                            <span>{initials}</span>
                          )}
                        </div>
                      );
                    })
                  ) : (
                    <span className="text-xs text-slate-400 py-1.5 pl-1 font-medium">Unassigned</span>
                  )}
                </div>

                {/* Dropdown triggers */}
                <div className="relative">
                  <button
                    type="button"
                    onClick={() => setIsDropdownOpen(!isDropdownOpen)}
                    className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-indigo-700 hover:text-indigo-800 bg-indigo-50 hover:bg-indigo-100 border border-indigo-100 rounded-full transition-colors focus:outline-none"
                  >
                    Manage Staff
                    <ChevronDown className="w-3.5 h-3.5" />
                  </button>

                  {isDropdownOpen && (
                    <div className="absolute left-0 mt-2 w-64 bg-white border border-slate-200 rounded-xl shadow-lg z-20 overflow-hidden animate-in fade-in slide-in-from-top-1 duration-150">
                      <div className="p-2.5 border-b border-slate-100 bg-slate-50/50 flex justify-between items-center">
                        <span className="text-xs font-bold text-slate-600 px-1">Workspace Members</span>
                        <button
                          type="button"
                          onClick={() => setIsDropdownOpen(false)}
                          className="text-[10px] text-indigo-600 hover:underline font-bold"
                        >
                          Done
                        </button>
                      </div>
                      
                      <div className="max-h-56 overflow-y-auto p-1 divide-y divide-slate-50">
                        {workspaceLoading ? (
                          <div className="flex items-center justify-center p-4">
                            <Loader2 className="w-4 h-4 text-indigo-500 animate-spin" />
                          </div>
                        ) : availableMembers && availableMembers.length > 0 ? (
                          availableMembers.map((member) => {
                            const isAssigned = currentAssignees?.some((a) => a.id === member.id) || false;
                            const initials = member.fullName
                              ? member.fullName.split(' ').map((n: string) => n[0]).join('').slice(0, 2).toUpperCase()
                              : '?';
                            const isMutating =
                              (assignMutation.isPending && assignMutation.variables === member.id) ||
                              (unassignMutation.isPending && unassignMutation.variables === member.id);

                            return (
                              <button
                                type="button"
                                key={member.id}
                                onClick={() => handleToggleAssignee(member.id, isAssigned)}
                                disabled={isMutating}
                                className="w-full flex items-center justify-between gap-3 p-2 rounded-lg hover:bg-slate-50 text-left transition-colors focus:outline-none group/row disabled:opacity-50"
                              >
                                <div className="flex items-center gap-2.5 min-w-0">
                                  <div className="relative shrink-0 h-7 w-7 rounded-full bg-slate-100 border border-slate-200 flex items-center justify-center text-[10px] font-bold text-slate-600">
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
                                  </div>
                                  <div className="flex flex-col min-w-0">
                                    <span className="text-xs font-bold text-slate-700 truncate group-hover/row:text-slate-900 leading-normal">
                                      {member.fullName}
                                    </span>
                                    {member.email && (
                                      <span className="text-[10px] text-slate-400 truncate">
                                        {member.email}
                                      </span>
                                    )}
                                  </div>
                                </div>
                                
                                {isMutating ? (
                                  <Loader2 className="w-3.5 h-3.5 text-indigo-500 animate-spin" />
                                ) : isAssigned ? (
                                  <Check className="w-4 h-4 text-emerald-600 shrink-0" />
                                ) : null}
                              </button>
                            );
                          })
                        ) : (
                          <div className="text-xs text-slate-400 p-4 text-center">No other members in workspace</div>
                        )}
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </div>
          )}

          <div className="pt-4 flex flex-col sm:flex-row gap-3 items-center justify-between border-t border-slate-100">
            {mode === 'edit' ? (
              <button
                type="button"
                onClick={handleDelete}
                disabled={isPending}
                className="flex items-center gap-1.5 text-sm font-semibold text-red-600 hover:text-red-700 hover:bg-red-50 p-2 rounded-lg transition-colors w-full sm:w-auto justify-center sm:justify-start disabled:opacity-50"
              >
                <Trash2 className="w-4 h-4" />
                Delete Task
              </button>
            ) : <div />}
            
            <div className="flex gap-3 w-full sm:w-auto">
              <button
                type="button"
                onClick={onClose}
                disabled={isPending}
                className="flex-1 sm:flex-none px-4 py-2 text-sm font-semibold text-slate-700 bg-white border border-slate-300 rounded-lg shadow-sm hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-indigo-500/30 transition-colors disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={isPending || !title.trim()}
                className="flex-1 sm:flex-none inline-flex items-center justify-center min-w-25 px-4 py-2 text-sm font-semibold text-white bg-indigo-600 rounded-lg shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500/30 focus:ring-offset-1 transition-all disabled:opacity-70 disabled:cursor-not-allowed"
              >
                {isPending ? (
                  <>
                    <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                    Saving...
                  </>
                ) : mode === 'create' ? 'Create' : 'Save Changes'}
              </button>
            </div>
          </div>
        </form>
      </div>
    </div>
  );
}
