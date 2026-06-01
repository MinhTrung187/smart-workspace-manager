import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { getMyInvitations, sendInvitation, acceptInvitation } from '../api/workspaceInvitationApi';

export const useMyInvitationsQuery = () => {
  return useQuery({
    queryKey: ['myInvitations'],
    queryFn: getMyInvitations,
  });
};

export const useSendInvitationMutation = (workspaceId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (email: string) => sendInvitation(workspaceId, email),
    onSuccess: () => {
      // Invalidate workspace query to keep it in sync if needed, though members don't join immediately
      queryClient.invalidateQueries({ queryKey: ['workspace', workspaceId] });
    },
  });
};

export const useAcceptInvitationMutation = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (invitationId: string) => acceptInvitation(invitationId),
    onSuccess: () => {
      // Invalidate workspaces list to show the new workspace immediately
      queryClient.invalidateQueries({ queryKey: ['workspaces'] });
      // Invalidate notifications/invitations query to remove the joined card from outstanding inbox
      queryClient.invalidateQueries({ queryKey: ['myInvitations'] });
    },
  });
};
