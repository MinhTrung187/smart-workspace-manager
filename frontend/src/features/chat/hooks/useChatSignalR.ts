import { useEffect } from 'react';
import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import type { ChatMessageDto } from '../types';

const getCurrentUserId = (): string | null => {
  const token = localStorage.getItem('accessToken');
  if (!token) return null;
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      window
        .atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    const parsed = JSON.parse(jsonPayload);
    return parsed.sub || null;
  } catch (e) {
    return null;
  }
};

export function useChatSignalR(channelId: string) {
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!channelId) return;

    const apiBaseUrl = (import.meta as any).env.VITE_API_BASE_URL ;
    const hubUrl = apiBaseUrl.replace(/\/api\/?$/, '') + '/hubs/chat';
    const token = localStorage.getItem('accessToken');

    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => token || '',
      })
      .withAutomaticReconnect()
      .build();

    const startConnection = async () => {
      try {
        await connection.start();
        console.log(`[SignalR] Connected successfully to ChatHub!`);
        await connection.invoke('JoinChannel', channelId);
        console.log(`[SignalR] Joined real-time group for channel: ${channelId}`);
      } catch (err) {
        console.error('[SignalR] Failed to start ChatHub connection:', err);
      }
    };

    startConnection();

    connection.on('ReceiveChatMessage', (message: any) => {
      console.log('[SignalR] Received chat message:', message);
      const currentUserId = getCurrentUserId();

      queryClient.setQueryData<ChatMessageDto[]>(['chatMessages', channelId], (oldMessages) => {
        if (!oldMessages) return [
          {
            ...message,
            sentByMe: message.userId === currentUserId
          }
        ];

        if (oldMessages.some((m) => m.id === message.id)) {
          return oldMessages;
        }

        return [
          ...oldMessages,
          {
            ...message,
            sentByMe: message.userId === currentUserId
          }
        ];
      });
    });

    return () => {
      const stopConnection = async () => {
        if (connection.state === HubConnectionState.Connected) {
          try {
            await connection.invoke('LeaveChannel', channelId);
            console.log(`[SignalR] Left real-time group for channel: ${channelId}`);
          } catch (err) {
            console.error('[SignalR] Error leaving channel group:', err);
          }
        }
        try {
          await connection.stop();
          console.log('[SignalR] ChatHub connection closed.');
        } catch (err) {
          console.error('[SignalR] Error closing ChatHub connection:', err);
        }
      };

      stopConnection();
    };
  }, [channelId, queryClient]);
}
