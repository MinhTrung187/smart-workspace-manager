import axios from 'axios';
import type { LoginRequest, RegisterRequest, AuthResponse } from '../types';

const API_BASE_URL = (import.meta as any).env.VITE_API_BASE_URL ;

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const loginUser = async (data: LoginRequest): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>('/Auth/login', data);
  return response.data;
};

export const registerUser = async (data: RegisterRequest): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>('/Auth/register', data);
  return response.data;
};
