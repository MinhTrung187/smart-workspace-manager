import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createColumn, updateColumn, deleteColumn, moveColumn } from '../api/columnApi';
import type { CreateColumnRequest, UpdateColumnRequest } from '../types';

export const useCreateColumn = (boardId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: Omit<CreateColumnRequest, 'boardId'>) =>
      createColumn({ ...data, boardId }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
    },
  });
};

export const useUpdateColumn = (boardId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateColumnRequest }) =>
      updateColumn(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
    },
  });
};

export const useDeleteColumn = (boardId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteColumn(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
    },
  });
};
export const useMoveColumn = (boardId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, newIndex }: { id: string; newIndex: number }) =>
      moveColumn(id, newIndex),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
    },
  });
};