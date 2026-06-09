import { useState, useEffect } from 'react';
import type { FormEvent } from 'react';
import { X, Calendar, Flag, Loader2, Trash2, UserPlus, Check, ChevronDown, Paperclip, Download, File, Eye, MessageSquare, Send } from 'lucide-react';
import { useQueryClient } from '@tanstack/react-query';
import type { TaskDto } from '../types';
import {
  useCreateTask,
  useUpdateTask,
  useDeleteTask,
  useTaskAssigneesQuery,
  useAssignUserMutation,
  useUnassignUserMutation,
  useTaskAttachmentsQuery,
  useUploadAttachmentMutation,
  useDeleteAttachmentMutation,
  useTaskCommentsQuery,
  useAddTaskCommentMutation,
  useDeleteTaskCommentMutation
} from '../hooks/useTaskMutations';
import { useBoardDetailQuery } from '../../board/hooks/useBoard';
import { useWorkspaceMembersQuery } from '../../workspace/hooks/useWorkspace';
import { assignUserToTask } from '../api/taskApi';
import { useCommentSignalR } from '../hooks/useCommentSignalR';

interface TaskModalProps {
  isOpen: boolean;
  onClose: () => void;
  boardId: string;
  mode: 'create' | 'edit';
  columnId: string;
  task?: TaskDto | null;
}

export default function TaskModal({ isOpen, onClose, boardId, mode, columnId, task }: TaskModalProps) {
  const getCurrentUserId = (): string | null => {
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
      return parsed.id || parsed.userId || parsed.sub || parsed.nameid || null;
    } catch (e) {
      return null;
    }
  };
  const queryClient = useQueryClient();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [dueDate, setDueDate] = useState('');
  const [priority, setPriority] = useState<'Low' | 'Medium' | 'High'>('Medium');
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const [isSubmittingTask, setIsSubmittingTask] = useState(false);

  // Create mode selected assignees state
  const [selectedUserIds, setSelectedUserIds] = useState<string[]>([]);

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

  // Fetch workspace members using workspace members list API
  const { data: availableMembers, isLoading: workspaceMembersLoading } = useWorkspaceMembersQuery(workspaceId);

  const assignMutation = useAssignUserMutation(boardId, task?.id || '');
  const unassignMutation = useUnassignUserMutation(boardId, task?.id || '');

  // Task attachments query and mutations
  const { data: attachments, isLoading: attachmentsLoading } = useTaskAttachmentsQuery(
    mode === 'edit' && task ? task.id : ''
  );
  const uploadAttachmentMutation = useUploadAttachmentMutation(boardId, task?.id || '');
  const deleteAttachmentMutation = useDeleteAttachmentMutation(boardId, task?.id || '');
  // Task comments query and mutations
  const { data: comments, isLoading: commentsLoading } = useTaskCommentsQuery(
    mode === 'edit' && task ? task.id : ''
  );
  const addCommentMutation = useAddTaskCommentMutation(task?.id || '');
  const deleteCommentMutation = useDeleteTaskCommentMutation(task?.id || '');

  // Establish real-time SignalR connection for comments
  useCommentSignalR(mode === 'edit' && task ? task.id : '');

  // Comment input state
  const [commentText, setCommentText] = useState('');
  const currentUserId = getCurrentUserId();

  const formatBytes = (bytes: number, decimals = 2) => {
    if (!bytes || bytes === 0) return '0 Bytes';
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
  };

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

  const handleToggleAssigneeCreate = (userId: string) => {
    setSelectedUserIds((prev) =>
      prev.includes(userId) ? prev.filter((id) => id !== userId) : [...prev, userId]
    );
  };

  useEffect(() => {
    if (isOpen) {
      setIsDropdownOpen(false);
      setSelectedUserIds([]);
      setIsSubmittingTask(false);
      setCommentText('');

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
    if (!title.trim() || isSubmittingTask) return;

    let isoDueDate: string | null = null;
    if (dueDate) {
      isoDueDate = new Date(dueDate).toISOString();
    }

    if (mode === 'create') {
      setIsSubmittingTask(true);
      createTaskMutation.mutate(
        {
          columnId,
          title: title.trim(),
          description: description.trim(),
          dueDate: isoDueDate,
          priority,
        },
        {
          onSuccess: async (createdTask) => {
            try {
              // Sequentially perform assignUserToTask calls for any selected user IDs in creation mode
              if (selectedUserIds.length > 0) {
                for (const userId of selectedUserIds) {
                  await assignUserToTask(createdTask.id, userId);
                }
              }
            } catch (err) {
              console.error('Error assigning users while creating task:', err);
            } finally {
              setIsSubmittingTask(false);
              // Refreshes the board data (columns, tasks, assignees)
              queryClient.invalidateQueries({ queryKey: ['board', boardId] });
              onClose();
            }
          },
          onError: () => {
            setIsSubmittingTask(false);
          },
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

  const isPending = createTaskMutation.isPending || updateTaskMutation.isPending || deleteTaskMutation.isPending || isSubmittingTask;

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm animate-in fade-in duration-200">
      <div className={`bg-white rounded-2xl shadow-xl w-full ${mode === 'edit' && task ? 'max-w-5xl' : 'max-w-lg'} overflow-hidden animate-in zoom-in-95 duration-200 border border-slate-200`}>
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

        <form onSubmit={handleSubmit} className="flex flex-col h-full max-h-[85vh] overflow-hidden bg-white">
          <div className="flex-1 overflow-y-auto p-6">
            <div className={mode === 'edit' && task ? 'grid grid-cols-1 lg:grid-cols-3 gap-8' : 'space-y-5'}>
              <div className={mode === 'edit' && task ? 'lg:col-span-2 space-y-5' : 'space-y-5'}>            <div>
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
                <div className="border-t border-slate-100 pt-4">
                  <label className="block text-xs font-bold uppercase tracking-wider text-slate-500 mb-2 items-center gap-1.5">
                    <UserPlus className="w-3.5 h-3.5" /> Task Assignees
                  </label>

                  <div className="flex flex-wrap items-center gap-3">
                    {/* Stacked list of current assignees */}
                    <div className="flex -space-x-2 overflow-hidden">
                      {mode === 'edit' ? (
                        assigneesLoading ? (
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
                        )
                      ) : (
                        // Create Mode UI Representations of Selected Members
                        selectedUserIds.length > 0 ? (
                          availableMembers
                            ?.filter((member) => selectedUserIds.includes(member.userId))
                            ?.map((member) => {
                              const initials = member.fullName
                                ? member.fullName.split(' ').map((n) => n[0]).join('').slice(0, 2).toUpperCase()
                                : '?';
                              return (
                                <div
                                  key={member.userId}
                                  title={member.fullName}
                                  className="relative group h-8 w-8 rounded-full bg-indigo-500 border-2 border-white flex items-center justify-center text-[10px] font-bold text-white shadow-sm"
                                >
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
                              );
                            })
                        ) : (
                          <span className="text-xs text-slate-400 py-1.5 pl-1 font-medium">Unassigned</span>
                        )
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
                        <div className="absolute left-0 mt-2 w-64 bg-white border border-slate-200 rounded-xl shadow-lg z-25 overflow-hidden animate-in fade-in slide-in-from-top-1 duration-150">
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
                            {workspaceMembersLoading ? (
                              <div className="flex items-center justify-center p-4">
                                <Loader2 className="w-4 h-4 text-indigo-500 animate-spin" />
                              </div>
                            ) : availableMembers && availableMembers.length > 0 ? (
                              availableMembers.map((member) => {
                                const isAssigned = mode === 'edit'
                                  ? (currentAssignees?.some((a) => a.id === member.userId || a.id === member.id) || false)
                                  : selectedUserIds.includes(member.userId);

                                const initials = member.fullName
                                  ? member.fullName.split(' ').map((n: string) => n[0]).join('').slice(0, 2).toUpperCase()
                                  : '?';

                                const isMutating = mode === 'edit' && (
                                  (assignMutation.isPending && assignMutation.variables === member.userId) ||
                                  (unassignMutation.isPending && unassignMutation.variables === member.userId)
                                );

                                return (
                                  <button
                                    type="button"
                                    key={member.userId}
                                    onClick={() => {
                                      if (mode === 'edit') {
                                        handleToggleAssignee(member.userId, isAssigned);
                                      } else {
                                        handleToggleAssigneeCreate(member.userId);
                                      }
                                    }}
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

                {/* Attachments Section */}
                {mode === 'edit' && task && (
                  <div className="border-t border-slate-100 pt-4">
                    <label className="block text-xs font-bold uppercase tracking-wider text-slate-500 mb-2.5 items-center gap-1.5">
                      <Paperclip className="w-3.5 h-3.5" /> Task Attachments
                    </label>

                    {/* Upload Dropzone */}
                    <div
                      onDragOver={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                      }}
                      onDrop={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        const file = e.dataTransfer.files?.[0];
                        if (file) {
                          uploadAttachmentMutation.mutate(file);
                        }
                      }}
                      onClick={() => {
                        document.getElementById('task-file-input')?.click();
                      }}
                      className="border border-dashed border-slate-300 hover:border-indigo-500 rounded-xl p-4 text-center cursor-pointer bg-slate-50 hover:bg-indigo-50/20 transition-all relative group"
                    >
                      <input
                        id="task-file-input"
                        type="file"
                        className="hidden"
                        onChange={(e) => {
                          const file = e.target.files?.[0];
                          if (file) {
                            uploadAttachmentMutation.mutate(file);
                          }
                        }}
                      />

                      <div className="flex flex-col items-center justify-center gap-1.5">
                        <div className="h-8 w-8 rounded-full bg-white shadow-sm border border-slate-200 flex items-center justify-center text-slate-500 group-hover:text-indigo-600 group-hover:border-indigo-200 transition-all">
                          <Paperclip className="w-4 h-4" />
                        </div>
                        <div className="text-xs font-semibold text-slate-700">
                          Click or Drag & Drop to attach a file
                        </div>
                        <div className="text-[10px] text-slate-400">
                          PDF, Image, Doc up to 10MB
                        </div>
                      </div>

                      {uploadAttachmentMutation.isPending && (
                        <div className="absolute inset-0 bg-white/80 backdrop-blur-[1px] rounded-xl flex items-center justify-center gap-2">
                          <Loader2 className="w-5 h-5 text-indigo-600 animate-spin" />
                          <span className="text-xs font-bold text-slate-700">Uploading file...</span>
                        </div>
                      )}
                    </div>

                    {/* Attachments List */}
                    <div className="mt-3.5 space-y-2">
                      {attachmentsLoading ? (
                        <div className="flex items-center gap-2 text-xs text-slate-400 font-semibold p-2">
                          <Loader2 className="w-3.5 h-3.5 text-indigo-500 animate-spin" />
                          Loading attachments...
                        </div>
                      ) : attachments && attachments.length > 0 ? (
                        attachments.map((attach) => {
                          const isDeleting = deleteAttachmentMutation.isPending && deleteAttachmentMutation.variables === attach.id;

                          return (
                            <div
                              key={attach.id}
                              className={`flex items-center justify-between p-2 hover:bg-slate-50 dark:hover:bg-neutral-800/10 rounded-lg transition-colors border border-slate-200/90 bg-white group hover:border-slate-300 ${isDeleting ? 'opacity-55' : ''
                                }`}
                            >
                              <div className="flex items-center gap-3 min-w-0">
                                <div className="p-2 bg-slate-50 text-slate-500 border border-slate-200/60 rounded-lg group-hover:bg-indigo-50/48 group-hover:text-indigo-600 group-hover:border-indigo-100 transition-colors">
                                  <File className="w-4 h-4" />
                                </div>
                                <div className="min-w-0 flex flex-col">
                                  <span className="text-xs font-bold text-slate-700 truncate max-w-45 sm:max-w-50 md:max-w-70" title={attach.fileName}>
                                    {attach.fileName}
                                  </span>
                                  <span className="text-[10px] text-slate-400 font-medium leading-none mt-1">
                                    {attach.uploadedByName} • {formatBytes(attach.fileSize)} • {new Date(attach.createdAt).toLocaleDateString()}
                                  </span>
                                </div>
                              </div>

                              <div className="flex items-center gap-1.5 opacity-0 group-hover:opacity-100 transition-opacity">
                                <button
                                  type="button"
                                  onClick={() => window.open(attach.fileUrl, '_blank', 'noopener,noreferrer')}
                                  className="p-1.5 text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors cursor-pointer"
                                  title="Preview file"
                                >
                                  <Eye className="w-4 h-4" />
                                </button>
                                <a
                                  href={attach.fileUrl}
                                  target="_blank"
                                  rel="noopener noreferrer"
                                  download={attach.fileName}
                                  className="p-1.5 text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors cursor-pointer"
                                  title="Download File"
                                >
                                  <Download className="w-4 h-4" />
                                </a>
                                <button
                                  type="button"
                                  onClick={() => deleteAttachmentMutation.mutate(attach.id)}
                                  disabled={isDeleting}
                                  className="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer"
                                  title="Delete File"
                                >
                                  {isDeleting ? (
                                    <Loader2 className="w-4 h-4 text-red-600 animate-spin" />
                                  ) : (
                                    <Trash2 className="w-4 h-4" />
                                  )}
                                </button>
                              </div>
                            </div>
                          );
                        })
                      ) : !uploadAttachmentMutation.isPending && (
                        <div className="text-center p-3 text-slate-400 text-[11px] font-medium border border-dotted border-slate-200 rounded-xl bg-slate-50/30">
                          No files attached yet.
                        </div>
                      )}
                    </div>
                  </div>
                )}
              </div>

              {/* Discussion / Comments Section */}
              {mode === 'edit' && task && (
                <div className="lg:col-span-1 border-t lg:border-t-0 lg:border-l border-slate-100 pl-0 lg:pl-6 pt-6 lg:pt-0 flex flex-col h-full bg-white">
                  <div className="flex items-center justify-between pb-3 border-b border-slate-100 mb-4 shrink-0">
                    <h3 className="text-sm font-bold text-slate-800 flex items-center gap-1.5 uppercase tracking-wider">
                      <MessageSquare className="w-4 h-4 text-slate-500" />
                      Discussion
                    </h3>
                    <span className="px-2 py-0.5 text-xs font-bold bg-slate-100 text-slate-600 rounded-full">
                      {comments?.length || 0}
                    </span>
                  </div>

                  {/* New Comment Input */}
                  <div className="mb-4 shrink-0">
                    <div className="relative">
                      <textarea
                        rows={2}
                        placeholder="Write a comment..."
                        value={commentText}
                        onChange={(e) => setCommentText(e.target.value)}
                        disabled={addCommentMutation.isPending}
                        className="block w-full px-3 py-2 text-slate-900 bg-white border border-slate-300 rounded-xl shadow-sm placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/30 focus:border-indigo-500 transition-shadow sm:text-xs resize-none pr-10"
                        onKeyDown={(e) => {
                          if (e.key === 'Enter' && !e.shiftKey) {
                            e.preventDefault();
                            if (!commentText.trim() || addCommentMutation.isPending) return;
                            addCommentMutation.mutate(commentText.trim(), {
                              onSuccess: () => setCommentText(''),
                            });
                          }
                        }}
                      />
                      <button
                        type="button"
                        onClick={() => {
                          if (!commentText.trim() || addCommentMutation.isPending) return;
                          addCommentMutation.mutate(commentText.trim(), {
                            onSuccess: () => setCommentText(''),
                          });
                        }}
                        disabled={addCommentMutation.isPending || !commentText.trim()}
                        className="absolute right-2.5 bottom-2.5 p-1 text-slate-400 hover:text-indigo-600 disabled:opacity-50 transition-colors disabled:hover:text-slate-400 cursor-pointer"
                        title="Post comment"
                      >
                        {addCommentMutation.isPending ? (
                          <Loader2 className="w-4 h-4 animate-spin text-indigo-600" />
                        ) : (
                          <Send className="w-4 h-4" />
                        )}
                      </button>
                    </div>
                    <div className="text-[10px] text-slate-400 mt-1 pl-1">
                      Press <kbd className="font-sans font-semibold">Enter</kbd> to post.
                    </div>
                  </div>

                  {/* Comments Timeline */}
                  <div className="flex-1 overflow-y-auto space-y-3.5 pr-1 max-h-87.5">
                    {commentsLoading ? (
                      <div className="flex items-center gap-2 text-xs text-slate-400 font-semibold p-2">
                        <Loader2 className="w-3.5 h-3.5 text-indigo-500 animate-spin" />
                        Loading comments...
                      </div>
                    ) : comments && comments.length > 0 ? (
                      comments.map((comment) => {
                        const isCommentDeleting = deleteCommentMutation.isPending && deleteCommentMutation.variables === comment.id;
                        const initials = comment.userName
                          ? comment.userName.split(' ').map((n) => n[0]).join('').slice(0, 2).toUpperCase()
                          : '?';
                        const isOwner = currentUserId === comment.userId;

                        return (
                          <div
                            key={comment.id}
                            className={`flex gap-3 text-xs leading-normal items-start group relative ${isCommentDeleting ? 'opacity-55' : ''
                              }`}
                          >
                            {/* User Avatar */}
                            <div className="shrink-0 h-7 w-7 rounded-full bg-indigo-600 border border-indigo-250 flex items-center justify-center text-[10px] font-bold text-white shadow-sm overflow-hidden select-none">
                              {comment.avatarUrl ? (
                                <img
                                  src={comment.avatarUrl}
                                  alt={comment.userName}
                                  className="h-full w-full object-cover"
                                  referrerPolicy="no-referrer"
                                />
                              ) : (
                                <span>{initials}</span>
                              )}
                            </div>

                            {/* Comment Bubble */}
                            <div className="flex-1 min-w-0">
                              <div className="flex items-baseline justify-between gap-2.5">
                                <span className="font-bold text-slate-800 truncate">
                                  {comment.userName}
                                </span>
                                <span className="text-[9px] text-slate-400 font-medium whitespace-nowrap">
                                  {new Date(comment.createdAt).toLocaleDateString()} {new Date(comment.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                </span>
                              </div>

                              <div className="mt-1 p-2.5 bg-slate-50 border border-slate-100 rounded-xl text-slate-700 wrap-break-word whitespace-pre-wrap font-medium">
                                {comment.content}
                              </div>
                            </div>

                            {/* Trash action for deletion */}
                            {isOwner && (
                              <button
                                type="button"
                                onClick={() => deleteCommentMutation.mutate(comment.id)}
                                disabled={isCommentDeleting}
                                className="absolute top-0 right-0 p-1 text-slate-400 hover:text-red-400 hover:bg-red-50 rounded opacity-0 group-hover:opacity-100 transition-opacity cursor-pointer inline-flex items-center"
                                title="Delete comment"
                              >
                                {isCommentDeleting ? (
                                  <Loader2 className="w-3 h-3 text-red-500 animate-spin" />
                                ) : (
                                  <Trash2 className="w-3 h-3" />
                                )}
                              </button>
                            )}
                          </div>
                        );
                      })
                    ) : (
                      <div className="text-center p-4 text-slate-400 text-[11px] font-medium border border-dotted border-slate-200 rounded-xl bg-slate-50/30">
                        No discussion yet. Start the conversation!
                      </div>
                    )}
                  </div>
                </div>
              )}

            </div>
          </div>

          <div className="p-5 border-t border-slate-100 bg-slate-50/50 flex flex-col sm:flex-row gap-3 items-center justify-between shrink-0">
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