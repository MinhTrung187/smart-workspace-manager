import { axiosInstance as apiClient } from '../../../api/axiosInstance';
import type { TaskDto, CreateTaskRequest, UpdateTaskRequest, TaskAssigneeDto, TaskAttachmentDto, TaskCommentDto } from '../types';


export const createTask = async (data: CreateTaskRequest): Promise<TaskDto> => {
  const response = await apiClient.post<TaskDto>('/BoardTask', data);
  return response.data;
};

export const updateTask = async (id: string, data: UpdateTaskRequest): Promise<void> => {
  await apiClient.put(`/BoardTask/${id}`, data);
};

export const deleteTask = async (id: string): Promise<void> => {
  await apiClient.delete(`/BoardTask/${id}`);
};

export const moveTask = async (id: string, targetColumnId: string, newIndex: number): Promise<void> => {
  await apiClient.patch(`/BoardTask/${id}/move`, { targetColumnId, newIndex });
};

export const getTaskAssignees = async (taskId: string): Promise<TaskAssigneeDto[]> => {
  const response = await apiClient.get<any[]>(`/TaskAssignee/task/${taskId}`);
  return response.data.map((item) => ({
    id: item.userId || item.id,
    userId: item.userId,
    fullName: item.fullName,
    avatarUrl: item.avatarUrl,
  }));
};

export const assignUserToTask = async (taskId: string, userId: string): Promise<void> => {
  await apiClient.post('/TaskAssignee/assign', { taskId, userId });
};

export const unassignUserFromTask = async (taskId: string, userId: string): Promise<void> => {
  await apiClient.delete(`/TaskAssignee/${taskId}/assign/${userId}`);
};
export const getTaskAttachments = async (taskId: string): Promise<TaskAttachmentDto[]> => {
  const response = await apiClient.get<TaskAttachmentDto[]>(`/TaskAttachments/task/${taskId}`);
  return response.data;
};

export const uploadTaskAttachment = async (taskId: string, file: File): Promise<TaskAttachmentDto> => {
  const formData = new FormData();
  formData.append('file', file);
  const response = await apiClient.post<TaskAttachmentDto>(`/TaskAttachments/${taskId}`, formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });
  return response.data;
};

export const deleteTaskAttachment = async (id: string): Promise<void> => {
  await apiClient.delete(`/TaskAttachments/${id}`);
};
export const getTaskComments = async (taskId: string): Promise<TaskCommentDto[]> => {
  const response = await apiClient.get<TaskCommentDto[]>(`/TaskComments/task/${taskId}`);
  return response.data;
};

export const addTaskComment = async (taskId: string, content: string): Promise<TaskCommentDto> => {
  const response = await apiClient.post<TaskCommentDto>(`/TaskComments/${taskId}`, { content });
  return response.data;
};

export const deleteTaskComment = async (id: string): Promise<void> => {
  await apiClient.delete(`/TaskComments/${id}`);
};