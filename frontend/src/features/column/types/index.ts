import type { TaskDto } from '../../task/types';

export interface ColumnDto {
  id: string;
  boardId: string;
  name: string;
  position: number;
  tasks: TaskDto[];
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
