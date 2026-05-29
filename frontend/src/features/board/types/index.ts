export interface BoardSummaryDto {
  id: string;
  workspaceId: string;
  name: string;
  createdByName: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateBoardRequest {
  workspaceId: string;
  name: string;
}

export interface BoardQueryOptions {
  tasksPage?: number;
  tasksPageSize?: number;
  taskSearch?: string;
  taskSortBy?: 'title' | 'dueDate' | 'priority' | 'createdAt' | 'updatedAt' | 'position';
  taskSortDesc?: boolean;
}
