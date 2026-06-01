import axios from 'axios';
import type { CreateWorkspaceRequest, GetAllWorkspacesResponse, WorkspaceDetailDto, WorkspaceMemberDto, WorkspaceSummaryDto } from '../types';

const API_BASE_URL = (import.meta as any).env.VITE_API_BASE_URL ;



const apiClient = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

apiClient.interceptors.request.use((config) => {
    const token = localStorage.getItem('accessToken');
    if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
}, (error) => {
    return Promise.reject(error);
});

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