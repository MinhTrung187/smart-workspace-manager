import { axiosInstance as apiClient } from '../../../api/axiosInstance';
import type { CreateBoardRequest, BoardQueryOptions, BoardSummaryDto } from '../types';
import type { BoardDetailResponse } from '../../column/types';




export const createBoard = async (data: CreateBoardRequest): Promise<BoardSummaryDto> => {
  const response = await apiClient.post<BoardSummaryDto>('/Board', data);
  return response.data;
};

export const getBoardById = async (id: string, options?: BoardQueryOptions): Promise<BoardDetailResponse> => {
  const response = await apiClient.get<BoardDetailResponse>(`/Board/${id}`, {
    params: options,
  });

  return response.data;
};
