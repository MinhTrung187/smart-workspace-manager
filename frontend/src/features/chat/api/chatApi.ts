import axios from 'axios';
import type { ChatMessageDto, SendMessageRequest } from '../types';

const API_BASE_URL = (import.meta as any).env.VITE_API_BASE_URL ;

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

export const getChannelMessages = async (channelId: string): Promise<ChatMessageDto[]> => {
  const response = await apiClient.get<ChatMessageDto[]>(`/chat/channels/${channelId}/messages`);
  return response.data;
};

export const sendMessage = async (channelId: string, content: string): Promise<ChatMessageDto> => {
  const body: SendMessageRequest = { content };
  const response = await apiClient.post<ChatMessageDto>(`/chat/channels/${channelId}/messages`, body);
  return response.data;
};
