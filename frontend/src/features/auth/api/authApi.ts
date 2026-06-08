import { axiosInstance as apiClient } from '../../../api/axiosInstance';
import type { LoginRequest, RegisterRequest, AuthResponse } from '../types';


export {apiClient};

export const loginUser = async (data: LoginRequest): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>('/Auth/login', data);
  return response.data;
};

export const registerUser = async (data: RegisterRequest): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>('/Auth/register', data);
  return response.data;
};
