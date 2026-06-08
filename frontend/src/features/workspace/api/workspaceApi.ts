import { axiosInstance as apiClient } from '../../../api/axiosInstance';
import type { CreateWorkspaceRequest, GetAllWorkspacesResponse, WorkspaceDetailDto, WorkspaceMemberDto, WorkspaceSummaryDto } from '../types';


export const getAllWorkspaces = async (): Promise<WorkspaceSummaryDto[]> => {
    const response = await apiClient.get<GetAllWorkspacesResponse>('/Workspace');
    return response.data.workspaces || [];
};

export const getWorkspaceById = async (id: string): Promise<WorkspaceDetailDto> => {
    const response = await apiClient.get<WorkspaceDetailDto>(`/Workspace/${id}`);
    return response.data;
};

export const initializeWorkspace = async (data: CreateWorkspaceRequest): Promise<WorkspaceDetailDto> => {
    const response = await apiClient.post<WorkspaceDetailDto>('/Workspace/initialize', data);
    return response.data;
};
export const getWorkspaceMembers = async (workspaceId: string): Promise<WorkspaceMemberDto[]> => {
  const response = await apiClient.get<WorkspaceMemberDto[]>(`/workspaces/${workspaceId}/members`);
  return response.data;
};