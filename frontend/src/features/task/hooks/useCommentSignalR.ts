import { useEffect } from 'react';
import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import type { TaskCommentDto } from '../types';

export function useCommentSignalR(taskId: string) {
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!taskId) return;

    const apiBaseUrl = (import.meta as any).env.VITE_API_BASE_URL;
    const hubUrl = apiBaseUrl.replace(/\/api\/?$/, '') + '/hubs/comment';
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
        console.log(`[SignalR] Connected successfully to CommentHub!`);
        await connection.invoke('JoinTask', taskId);
        console.log(`[SignalR] Joined real-time group for task: ${taskId}`);
      } catch (err) {
        console.error('[SignalR] Failed to start CommentHub connection:', err);
      }
    };

    startConnection();

    connection.on('CommentAdded', (comment: TaskCommentDto) => {
      console.log('[SignalR] Received CommentAdded:', comment);
      queryClient.setQueryData<TaskCommentDto[]>(['taskComments', taskId], (oldComments) => {
        if (!oldComments) return [comment];
        if (oldComments.some((c) => c.id === comment.id)) {
          return oldComments;
        }
        return [...oldComments, comment].sort(
          (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
        );
      });
    });

    connection.on('CommentDeleted', (commentId: string) => {
      console.log('[SignalR] Received CommentDeleted:', commentId);
      queryClient.setQueryData<TaskCommentDto[]>(['taskComments', taskId], (oldComments) => {
        if (!oldComments) return [];
        return oldComments.filter((c) => c.id !== commentId);
      });
    });

    return () => {
      const stopConnection = async () => {
        if (connection.state === HubConnectionState.Connected) {
          try {
            await connection.invoke('LeaveTask', taskId);
            console.log(`[SignalR] Left real-time group for task: ${taskId}`);
          } catch (err) {
            console.error('[SignalR] Error leaving task group:', err);
          }
        }
        try {
          await connection.stop();
          console.log('[SignalR] CommentHub connection closed.');
        } catch (err) {
          console.error('[SignalR] Error closing CommentHub connection:', err);
        }
      };

      stopConnection();
    };
  }, [taskId, queryClient]);
}
