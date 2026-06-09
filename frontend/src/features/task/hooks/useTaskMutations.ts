import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { createTask,
    updateTask,
    deleteTask,
    moveTask,
    getTaskAssignees, 
    assignUserToTask, 
    unassignUserFromTask, 
    deleteTaskAttachment, 
    uploadTaskAttachment, 
    getTaskAttachments,
    getTaskComments,
    addTaskComment,
    deleteTaskComment
   } from '../api/taskApi';
import type { CreateTaskRequest, UpdateTaskRequest } from '../types';

export const useCreateTask = (boardId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateTaskRequest) => createTask(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
    },
  });
};

export const useUpdateTask = (boardId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ taskId, data }: { taskId: string; data: UpdateTaskRequest }) =>
      updateTask(taskId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
    },
  });
};

export const useDeleteTask = (boardId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (taskId: string) => deleteTask(taskId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
    },
  });
};
export const useMoveTask = (boardId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, targetColumnId, newIndex }: { id: string; targetColumnId: string; newIndex: number }) =>
      moveTask(id, targetColumnId, newIndex),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
    },
  });
};

export const useTaskAssigneesQuery = (taskId: string) => {
  return useQuery({
    queryKey: ['taskAssignees', taskId],
    queryFn: () => getTaskAssignees(taskId),
    enabled: !!taskId,
  });
};

export const useAssignUserMutation = (boardId: string, taskId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (userId: string) => assignUserToTask(taskId, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['taskAssignees', taskId] });
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
    },
  });
};

export const useUnassignUserMutation = (boardId: string, taskId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (userId: string) => unassignUserFromTask(taskId, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['taskAssignees', taskId] });
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
    },
  });
};
export const useTaskAttachmentsQuery = (taskId: string) => {
  return useQuery({
    queryKey: ['taskAttachments', taskId],
    queryFn: () => getTaskAttachments(taskId),
    enabled: !!taskId,
  });
};

export const useUploadAttachmentMutation = (boardId: string, taskId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (file: File) => uploadTaskAttachment(taskId, file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['taskAttachments', taskId] });
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
    },
  });
};

export const useDeleteAttachmentMutation = (boardId: string, taskId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteTaskAttachment(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['taskAttachments', taskId] });
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
    },
  });
};
export const useTaskCommentsQuery = (taskId: string) => {
  return useQuery({
    queryKey: ['taskComments', taskId],
    queryFn: () => getTaskComments(taskId),
    enabled: !!taskId,
  });
};

export const useAddTaskCommentMutation = (taskId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (content: string) => addTaskComment(taskId, content),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['taskComments', taskId] });
    },
  });
};

export const useDeleteTaskCommentMutation = (taskId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteTaskComment(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['taskComments', taskId] });
    },
  });
};