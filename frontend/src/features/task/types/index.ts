export interface TaskAssigneeDto {
  id: string;
  userId?: string;
  fullName: string;
  avatarUrl?: string;
}

export interface TaskDto {
  id: string;
  columnId: string;
  title: string;
  description?: string;
  dueDate?: string | null;
  priority?: 'Low' | 'Medium' | 'High';
  position: number;
  assignees?: TaskAssigneeDto[];
}
export interface CreateTaskRequest {
  columnId: string;
  title: string;
  description?: string;
  dueDate?: string | null;
  priority?: 'Low' | 'Medium' | 'High';
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  dueDate?: string | null;
  priority?: 'Low' | 'Medium' | 'High';
  position: number;
  columnId: string;
}
