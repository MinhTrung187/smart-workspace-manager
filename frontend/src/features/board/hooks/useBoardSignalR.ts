import { useEffect } from 'react';
import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';

export function useBoardSignalR(boardId: string) {
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!boardId) return;

    // Retrieve backend API base URL or fallback to standard dev port
    const apiBaseUrl = (import.meta as any).env.VITE_API_BASE_URL || 'http://localhost:5194/api';
    
    // Construct the SignalR BoardHub connection URL (removes trailing /api or similar if present)
    const hubUrl = apiBaseUrl.replace(/\/api\/?$/, '') + '/hubs/board';

    const token = localStorage.getItem('accessToken');

    // Build SignalR connection
    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => token || '',
      })
      .withAutomaticReconnect()
      .build();

    const startConnection = async () => {
      try {
        await connection.start();
        console.log(`[SignalR] Connected successfully to BoardHub!`);

        // Join the board-specific group for targeted real-time broadcasts
        await connection.invoke('JoinBoard', boardId);
        console.log(`[SignalR] Joined real-time group for board: ${boardId}`);
      } catch (err) {
        console.error('[SignalR] Failed to start connection:', err);
      }
    };

    startConnection();

    // Invalidate the TanStack React Query cache when real-time updates are received
    const triggerCacheSync = (eventName: string) => {
      console.log(`[SignalR] Received [${eventName}] event. Refreshing board data...`);
      queryClient.invalidateQueries({ queryKey: ['board', boardId] });
    };

    // Set up listeners for board, column, and task real-time events
    connection.on('BoardUpdated', () => triggerCacheSync('BoardUpdated'));
    
    connection.on('ColumnCreated', () => triggerCacheSync('ColumnCreated'));
    connection.on('ColumnUpdated', () => triggerCacheSync('ColumnUpdated'));
    connection.on('ColumnDeleted', () => triggerCacheSync('ColumnDeleted'));
    connection.on('ColumnMoved', () => triggerCacheSync('ColumnMoved'));
    
    connection.on('TaskCreated', () => triggerCacheSync('TaskCreated'));
    connection.on('TaskUpdated', () => triggerCacheSync('TaskUpdated'));
    connection.on('TaskDeleted', () => triggerCacheSync('TaskDeleted'));
    connection.on('TaskMoved', () => triggerCacheSync('TaskMoved'));
    
    connection.on('ReceiveMessage', (message: string) => {
      console.log('[SignalR] Received message from hub:', message);
      // Trigger update as a safety fallback
      triggerCacheSync('ReceiveMessage');
    });

    // Cleanup: leave board group and close socket when component unmounts or boardId changes
    return () => {
      const stopConnection = async () => {
        if (connection.state === HubConnectionState.Connected) {
          try {
            await connection.invoke('LeaveBoard', boardId);
            console.log(`[SignalR] Left real-time group for board: ${boardId}`);
          } catch (err) {
            console.error('[SignalR] Error leaving board group:', err);
          }
        }
        try {
          await connection.stop();
          console.log('[SignalR] Connection closed.');
        } catch (err) {
          console.error('[SignalR] Error closing connection:', err);
        }
      };
      
      stopConnection();
    };
  }, [boardId, queryClient]);
}
