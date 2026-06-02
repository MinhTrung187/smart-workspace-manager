export interface ChatMessageDto {
  id: string;
  channelId: string;
  userId: string;
  userName: string;
  content: string;
  createdAt: string;
  editedAt: string | null;
  sentByMe: boolean;
}

export interface SendMessageRequest {
  content: string;
}
