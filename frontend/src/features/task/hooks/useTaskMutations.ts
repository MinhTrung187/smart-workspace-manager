import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createTask, updateTask, deleteTask } from '../api/taskApi';
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
