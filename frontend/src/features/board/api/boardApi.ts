import axios from 'axios';
import type { CreateBoardRequest, BoardSummaryDto } from '../types';

const API_BASE_URL = (import.meta as any).env.VITE_API_BASE_URL ;

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Automatically attach JWT token to every request
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
}, (error) => {
  return Promise.reject(error);
});

export const createBoard = async (data: CreateBoardRequest): Promise<BoardSummaryDto> => {
  const response = await apiClient.post<BoardSummaryDto>('/Board', data);
  return response.data;
};
