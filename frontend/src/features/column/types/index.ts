import type { TaskDto } from '../../task/types';

export interface ColumnDto {
  id: string;
  boardId: string;
  name: string;
  position: number;
  tasks: TaskDto[];
}
export interface CreateColumnRequest {
  boardId: string;
  name: string;
}

export interface UpdateColumnRequest {
  name: string;
  position: number;
}

export interface BoardDetailResponse {
  id: string;
  workspaceId: string;
  workspaceName?: string;
  name: string;
  createdByName: string;
  createdAt: string;
  columns: ColumnDto[];
}
