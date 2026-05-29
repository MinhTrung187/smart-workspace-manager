export interface TaskDto {
  id: string;
  columnId: string;
  title: string;
  description?: string;
  dueDate?: string | null;
  priority?: 'Low' | 'Medium' | 'High';
  position: number;
}
