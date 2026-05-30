import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import type { TaskDto } from '../types';
import { Calendar, Flag } from 'lucide-react';

interface TaskCardProps {
  key?: string | number;
  task: TaskDto;
  isOverlay?: boolean;
  onEdit?: () => void;
}

export default function TaskCard({ task, isOverlay, onEdit }: TaskCardProps) {
  const { setNodeRef, attributes, listeners, transform, transition, isDragging } = useSortable({
    id: task.id,
    data: {
      type: 'Task',
      task,
    },
  });

  const style = {
    transition,
    transform: CSS.Transform.toString(transform),
  };

  if (isDragging && !isOverlay) {
    return (
      <div
        ref={setNodeRef}
        style={style}
        className="h-25 w-full rounded-xl border-2 border-dashed border-indigo-300 bg-indigo-50 opacity-60"
      />
    );
  }

  const getPriorityColor = (priority?: string) => {
    switch (priority) {
      case 'High': return 'text-red-600 bg-red-50 ring-red-500/20';
      case 'Medium': return 'text-orange-600 bg-orange-50 ring-orange-500/20';
      case 'Low': return 'text-green-600 bg-green-50 ring-green-500/20';
      default: return 'text-slate-600 bg-slate-50 ring-slate-500/20';
    }
  };
    const getPriorityStyle = (priority?: string) => {
    switch (priority) {
      case 'High': return 'border-l-4 border-l-red-500 bg-red-50/30 dark:bg-red-950/5 border-y-slate-200 border-r-slate-200';
      case 'Medium': return 'border-l-4 border-l-amber-500 bg-amber-50/30 dark:bg-amber-950/5 border-y-slate-200 border-r-slate-200';
      case 'Low':
default:
  return 'border-l-4 border-l-green-500 bg-slate-50/20 dark:bg-green-950/5 border-y-slate-200 border-r-slate-200';
    }
  };

  return (
    <div
      ref={setNodeRef}
      style={style}
      onClick={onEdit}
      className={`relative flex flex-col gap-3 p-3.5 rounded-xl border shadow-sm hover:shadow-md transition-shadow group ${getPriorityStyle(task.priority)} ${isOverlay ? 'cursor-grabbing scale-[1.02] shadow-xl rotate-2 ring-1 ring-slate-200' : 'cursor-grab'}`}
      {...attributes}
      {...listeners}
    >
      <div className="flex justify-between items-start gap-2">
        <h4 className="text-sm font-bold text-slate-950 leading-snug">
          {task.title}
        </h4>
      </div>
      
      {task.description && (
        <p className="text-xs text-slate-600 line-clamp-2">
          {task.description}
        </p>
      )}

      {(task.priority || task.dueDate) && (
        <div className="flex items-center gap-2 mt-auto">
          {task.priority && (
            <span className={`inline-flex items-center px-2 py-0.5 rounded text-[10px] font-semibold tracking-wide leading-4 ring-1 ring-inset ${getPriorityColor(task.priority)}`}>
              {task.priority === 'High' && <Flag className="w-2.5 h-2.5 mr-1" />}
              {task.priority}
            </span>
          )}
          {task.dueDate && (
            <span className="inline-flex items-center px-1.5 py-0.5 rounded text-[10px] font-semibold tracking-wide leading-4 text-sky-700 bg-sky-50 ring-1 ring-inset ring-sky-200 gap-1">
              <Calendar className="w-3 h-3 text-sky-500" />
              {new Date(task.dueDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}
            </span>
          )}
        </div>
      )}
    </div>
  );
}
