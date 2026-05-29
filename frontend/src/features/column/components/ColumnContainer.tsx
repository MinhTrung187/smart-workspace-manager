import { useSortable, SortableContext, verticalListSortingStrategy } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { useMemo, useState, useRef, useEffect  } from 'react';
import type { KeyboardEvent } from 'react';
import type { TaskDto } from '../../task/types';
import type { ColumnDto } from '../types';
import TaskCard from '../../task/components/TaskCard';
import { Plus, GripHorizontal, Trash2 } from 'lucide-react';
import { useUpdateColumn, useDeleteColumn } from '../hooks/useColumnMutations';

interface ColumnContainerProps {
  key?: string | number;
  column: ColumnDto ;
  tasks: TaskDto[];
  isOverlay?: boolean;
}

export default function ColumnContainer({ column, tasks, isOverlay }: ColumnContainerProps) {
  const taskIds = useMemo(() => tasks.map((t) => t.id), [tasks]);
  const [isEditing, setIsEditing] = useState(false);
  const [editName, setEditName] = useState(column.name);
  const inputRef = useRef<HTMLInputElement>(null);

  const updateColumnMutation = useUpdateColumn(column.boardId);
  const deleteColumnMutation = useDeleteColumn(column.boardId);

  const { setNodeRef, attributes, listeners, transform, transition, isDragging } = useSortable({
    id: column.id,
    data: {
      type: 'Column',
      column,
    },
  });

  useEffect(() => {
    if (isEditing && inputRef.current) {
      inputRef.current.focus();
      inputRef.current.select();
    }
  }, [isEditing]);

  const style = {
    transition,
    transform: CSS.Transform.toString(transform),
  };

  if (isDragging && !isOverlay) {
    return (
      <div
        ref={setNodeRef}
        style={style}
        className="min-h-64 w-[320px] shrink-0 bg-indigo-50/70 rounded-2xl border-2 border-dashed border-indigo-400 opacity-60 flex flex-col"
      />
    );
  }

  const handleRenameSubmit = () => {
    setIsEditing(false);
    if (editName.trim() !== '' && editName !== column.name) {
      updateColumnMutation.mutate({
        id: column.id,
        data: { name: editName.trim(), position: column.position }
      });
    } else {
      setEditName(column.name);
    }
  };

  const handleKeyDown = (e: KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleRenameSubmit();
    } else if (e.key === 'Escape') {
      setIsEditing(false);
      setEditName(column.name);
    }
  };

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={`max-h-[calc(100vh-18rem)] min-h-64 w-[320px] shrink-0 overflow-hidden flex flex-col bg-white rounded-2xl border border-slate-300 shadow-sm ${isOverlay ? 'cursor-grabbing shadow-2xl scale-[1.02] rotate-1 ring-1 ring-indigo-200' : ''}`}
    >
      {/* Column Header */}
      <div 
        className="flex items-center justify-between p-4 group/header"
      >
        <div className="flex items-center gap-2 flex-1 outline-none min-w-0 pr-2">
          {isEditing ? (
            <input
              ref={inputRef}
              type="text"
              value={editName}
              onChange={(e) => setEditName(e.target.value)}
              onBlur={handleRenameSubmit}
              onKeyDown={handleKeyDown}
              className="flex-1 w-full bg-white border border-indigo-500 rounded px-2 py-0.5 text-sm font-bold text-slate-950 focus:outline-none focus:ring-2 focus:ring-indigo-500/20"
            />
          ) : (
            <h3 
              onClick={() => !isOverlay && setIsEditing(true)}
              className="font-bold text-slate-950 text-sm tracking-tight cursor-pointer truncate hover:text-indigo-600 transition-colors py-0.5 px-1 -ml-1 rounded"
              title={column.name}
            >
              {column.name}
            </h3>
          )}
          <span className="text-xs font-bold text-indigo-800 bg-indigo-100 ring-1 ring-inset ring-indigo-200 px-2 py-0.5 rounded-full shrink-0">
            {tasks.length}
          </span>
        </div>
        <div className="flex items-center gap-0.5 shrink-0 opacity-0 group-hover/header:opacity-100 transition-opacity">
          <button 
            onClick={() => {
              if (window.confirm('Are you sure you want to delete this column?')) {
                deleteColumnMutation.mutate(column.id);
              }
            }}
            className="p-1 text-slate-400 hover:text-red-600 hover:bg-slate-100 rounded-md transition-colors"
            title="Delete column"
          >
            <Trash2 className="w-4 h-4" />
          </button>
          <div {...attributes} {...listeners} className="p-1 text-slate-500 hover:text-indigo-700 hover:bg-slate-100 rounded-md transition-colors cursor-grab active:cursor-grabbing">
            <GripHorizontal className="w-4 h-4" />
          </div>
        </div>
      </div>

      {/* Task List */}
      <div className="flex-1 overflow-y-auto px-3 pb-2 flex flex-col gap-3 min-h-37.5">
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
