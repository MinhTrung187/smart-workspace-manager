import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { createTask, updateTask, deleteTask, moveTask, getTaskAssignees, assignUserToTask, unassignUserFromTask } from '../api/taskApi';
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
