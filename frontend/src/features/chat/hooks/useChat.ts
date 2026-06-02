import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getChannelMessages, sendMessage } from '../api/chatApi';

export const useChannelMessagesQuery = (channelId: string) => {
  return useQuery({
    queryKey: ['chatMessages', channelId],
    queryFn: () => getChannelMessages(channelId),
    enabled: !!channelId,
  });
};

export const useSendMessageMutation = (channelId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (content: string) => sendMessage(channelId, content),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['chatMessages', channelId] });
    },
  });
};
