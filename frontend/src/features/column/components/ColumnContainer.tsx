import { useSortable, SortableContext, verticalListSortingStrategy } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { useMemo } from 'react';
import type { ColumnDto } from '../types';
import type { TaskDto } from '../../task/types';
import TaskCard from '../../task/components/TaskCard';
import { Plus, GripHorizontal } from 'lucide-react';

interface ColumnContainerProps {
  column: ColumnDto;
  tasks: TaskDto[];
  isOverlay?: boolean;
}

export default function ColumnContainer({ column, tasks, isOverlay }: ColumnContainerProps) {
  const taskIds = useMemo(() => tasks.map((t) => t.id), [tasks]);

  const { setNodeRef, attributes, listeners, transform, transition, isDragging } = useSortable({
    id: column.id,
    data: {
      type: 'Column',
      column,
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
        className="min-h-[16rem] w-[320px] shrink-0 bg-indigo-50/70 rounded-2xl border-2 border-dashed border-indigo-400 opacity-60"
      />
    );
  }

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={`max-h-[calc(100vh-18rem)] min-h-[16rem] w-[320px] shrink-0 overflow-hidden flex flex-col bg-white rounded-2xl border border-slate-300 shadow-sm ${isOverlay ? 'cursor-grabbing shadow-2xl scale-[1.02] rotate-1 ring-1 ring-indigo-200' : ''}`}
    >
      {/* Column Header */}
      <div 
        {...attributes} 
        {...listeners} 
        className="flex items-center justify-between p-4 cursor-grab active:cursor-grabbing group/header"
      >
        <div className="flex items-center gap-2">
          <h3 className="font-bold text-slate-950 text-sm tracking-tight">{column.name}</h3>
          <span className="text-xs font-bold text-indigo-800 bg-indigo-100 ring-1 ring-inset ring-indigo-200 px-2 py-0.5 rounded-full">
            {tasks.length}
          </span>
        </div>
        <button className="p-1 text-slate-500 hover:text-indigo-700 opacity-0 group-hover/header:opacity-100 transition-opacity">
          <GripHorizontal className="w-4 h-4" />
        </button>
      </div>

      {/* Task List */}
      <div className="flex-1 min-h-[9.375rem] overflow-y-auto px-3 pb-2 flex flex-col gap-3">
        <SortableContext items={taskIds} strategy={verticalListSortingStrategy}>
          {tasks.map((task) => (
            <TaskCard key={task.id} task={task} />
          ))}
          {tasks.length === 0 && (
            <div className="flex items-center justify-center p-6 border-2 border-dashed border-indigo-300 bg-indigo-50/60 rounded-xl text-sm font-semibold text-indigo-400 mt-2">
              Drop tasks here
            </div>
          )}
        </SortableContext>
      </div>

      {/* Footer -> Add Task button */}
      <div className="p-3 mt-auto">
        <button className="flex items-center gap-1 w-full py-1.5 px-2 text-sm font-semibold text-indigo-600 hover:text-indigo-800 hover:bg-indigo-50 rounded-lg transition-colors">
          <Plus className="w-4 h-4" />
          Add task
        </button>
      </div>
    </div>
  );
}
