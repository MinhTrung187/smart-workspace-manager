export interface TaskDto {
  id: string;
  columnId: string;
  title: string;
  description?: string;
  dueDate?: string | null;
  priority?: 'Low' | 'Medium' | 'High';
  position: number;
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
