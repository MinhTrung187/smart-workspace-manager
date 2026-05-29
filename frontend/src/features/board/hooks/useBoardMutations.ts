import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createBoard } from '../api/boardApi';
import type { CreateBoardRequest } from '../types';

export const useCreateBoardMutation = (workspaceId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateBoardRequest) => createBoard(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workspace', workspaceId] });
    },
  });
};
