import { axiosInstance as apiClient } from '../../../api/axiosInstance';
import type { ColumnDto, CreateColumnRequest, UpdateColumnRequest } from '../types';



export const createColumn = async (data: CreateColumnRequest): Promise<ColumnDto> => {
  const response = await apiClient.post<ColumnDto>('/Column', data);
  return response.data;
};

export const updateColumn = async (id: string, data: UpdateColumnRequest): Promise<void> => {
  await apiClient.put(`/Column/${id}`, data);
};

export const deleteColumn = async (id: string): Promise<void> => {
  await apiClient.delete(`/Column/${id}`);
};
export const moveColumn = async (id: string, newIndex: number): Promise<void> => {
  await apiClient.patch(`/Column/${id}/move`, { newIndex });
};
