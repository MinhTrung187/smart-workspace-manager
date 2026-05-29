import { useQuery } from '@tanstack/react-query';
import { getBoardById } from '../api/boardApi';
import type { BoardQueryOptions } from '../types';

export const useBoardDetailQuery = (id: string, options?: BoardQueryOptions) => {
  return useQuery({
    queryKey: ['board', id, options],
    queryFn: () => getBoardById(id, options),
    enabled: !!id,
  });
};
