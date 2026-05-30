import axios from 'axios';
import type { TaskDto, CreateTaskRequest, UpdateTaskRequest } from '../types';

const API_BASE_URL = (import.meta as any).env.VITE_API_BASE_URL;

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
}, (error) => {
  return Promise.reject(error);
});
export const createTask = async (data: CreateTaskRequest): Promise<TaskDto> => {
  const response = await apiClient.post<TaskDto>('/BoardTask', data);
  return response.data;
};

export const updateTask = async (id: string, data: UpdateTaskRequest): Promise<void> => {
  await apiClient.put(`/BoardTask/${id}`, data);
};

export const deleteTask = async (id: string): Promise<void> => {
  await apiClient.delete(`/BoardTask/${id}`);
};

export const moveTask = async (id: string, targetColumnId: string, newIndex: number): Promise<void> => {
  await apiClient.patch(`/BoardTask/${id}/move`, { targetColumnId, newIndex });
};
