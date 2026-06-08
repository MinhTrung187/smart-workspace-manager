import { axiosInstance as apiClient } from '../../../api/axiosInstance';
import type { ChatMessageDto, SendMessageRequest } from '../types';


export const getChannelMessages = async (channelId: string): Promise<ChatMessageDto[]> => {
  const response = await apiClient.get<ChatMessageDto[]>(`/chat/channels/${channelId}/messages`);
  return response.data;
};

export const sendMessage = async (channelId: string, content: string): Promise<ChatMessageDto> => {
  const body: SendMessageRequest = { content };
  const response = await apiClient.post<ChatMessageDto>(`/chat/channels/${channelId}/messages`, body);
  return response.data;
};
