import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router';
import { getAllWorkspaces, getWorkspaceById, initializeWorkspace } from '../api/workspaceApi';
import type { CreateWorkspaceRequest } from '../types';

export const useWorkspacesQuery = () => {
  return useQuery({
    queryKey: ['workspaces'],
    queryFn: getAllWorkspaces,
  });
};

export const useWorkspaceDetailQuery = (id: string) => {
  return useQuery({
    queryKey: ['workspace', id],
    queryFn: () => getWorkspaceById(id),
    enabled: !!id,
  });
};

export const useInitializeWorkspaceMutation = () => {
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  return useMutation({
    mutationFn: (data: CreateWorkspaceRequest) => initializeWorkspace(data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['workspaces'] });
      navigate(`/workspaces/${data.id}`);
    },
  });
};
