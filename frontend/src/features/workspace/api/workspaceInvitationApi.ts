  import { axiosInstance as apiClient } from '../../../api/axiosInstance';

export interface WorkspaceInvitationDto {
  id: string;
  workspaceId: string;
  workspaceName: string;
  email: string;
  status: string;
  createdAt: string;
  expiredAt: string;
  link: string;
}

export interface SendInvitationResponse {
  id: string;
  workspaceId: string;
  email: string;
  status: string;
  expiredAt: string;
  inviteLink: string;
}

export interface AcceptInvitationResponse {
  userId: string;
  fullName: string;
  email: string;
  avatarUrl?: string;
  role: string;
  joinedAt: string;
}

export const getMyInvitations = async (): Promise<WorkspaceInvitationDto[]> => {
  const response = await apiClient.get<WorkspaceInvitationDto[]>('/WorkspaceInvitation');
  return response.data;
};

export const sendInvitation = async (workspaceId: string, email: string): Promise<SendInvitationResponse> => {
  const response = await apiClient.post<SendInvitationResponse>(`/WorkspaceInvitation/${workspaceId}/invite`, { email });
  return response.data;
};

export const acceptInvitation = async (id: string): Promise<AcceptInvitationResponse> => {
  const response = await apiClient.post<AcceptInvitationResponse>(`/WorkspaceInvitation/${id}/accept`);
  return response.data;
};
