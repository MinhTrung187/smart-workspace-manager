import axios from 'axios';
import type { ColumnDto, CreateColumnRequest, UpdateColumnRequest } from '../types';

const API_BASE_URL = (import.meta as any).env.VITE_API_BASE_URL ;

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

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
