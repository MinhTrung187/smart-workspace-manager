import { useState, useMemo, useEffect, useRef } from 'react';
import {
  DndContext,
  DragOverlay,
  closestCorners,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors
} from '@dnd-kit/core';
import type { FormEvent, KeyboardEvent } from 'react';
import type { DragStartEvent, DragOverEvent, DragEndEvent } from '@dnd-kit/core';
import { SortableContext, horizontalListSortingStrategy, arrayMove } from '@dnd-kit/sortable';
import type { BoardDetailResponse, ColumnDto } from '../../column/types';
import type { TaskDto } from '../../task/types';
import ColumnContainer from '../../column/components/ColumnContainer';
import TaskCard from '../../task/components/TaskCard';
import { Plus, X } from 'lucide-react';
import { useCreateColumn, useMoveColumn } from '../../column/hooks/useColumnMutations';
import { useMoveTask } from '../../task/hooks/useTaskMutations';
import TaskModal from '../../task/components/TaskModal';


interface KanbanBoardProps {
  board: BoardDetailResponse;
}

export default function KanbanBoard({ board }: KanbanBoardProps) {
  const [columns, setColumns] = useState<ColumnDto[]>([]);
  const [tasks, setTasks] = useState<TaskDto[]>([]);

  const [activeColumn, setActiveColumn] = useState<ColumnDto | null>(null);
  const [activeTask, setActiveTask] = useState<TaskDto | null>(null);

  const [isAddingColumn, setIsAddingColumn] = useState(false);
  const [newColumnName, setNewColumnName] = useState('');
  const addColumnInputRef = useRef<HTMLInputElement>(null);

  const [isTaskModalOpen, setIsTaskModalOpen] = useState(false);
  const [taskModalMode, setTaskModalMode] = useState<'create' | 'edit'>('create');
  const [taskModalColumnId, setTaskModalColumnId] = useState<string>('');
  const [taskModalTask, setTaskModalTask] = useState<TaskDto | null>(null);
  const createColumnMutation = useCreateColumn(board.id);
  const moveColumnMutation = useMoveColumn(board.id);
  const moveTaskMutation = useMoveTask(board.id);


  useEffect(() => {
    const sortedCols = [...(board.columns || [])].sort((a, b) => a.position - b.position);
    setColumns(sortedCols);

    const allTasks = sortedCols.flatMap(c => c.tasks || []);
    setTasks(allTasks.sort((a, b) => a.position - b.position));
  }, [board]);

  useEffect(() => {
    if (isAddingColumn && addColumnInputRef.current) {
      addColumnInputRef.current.focus();
    }
  }, [isAddingColumn]);

  const columnIds = useMemo(() => columns.map(col => col.id), [columns]);

  const normalizeTaskPositions = (taskList: TaskDto[]) => {
    const nextPositionByColumn = new Map<string, number>();

    return taskList.map(task => {
      const nextPosition = nextPositionByColumn.get(task.columnId) ?? 1000;
      nextPositionByColumn.set(task.columnId, nextPosition + 1000);

      return {
        ...task,
        position: nextPosition,
      };
    });
  };

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
    useSensor(KeyboardSensor)
  );

  const handleAddColumnSubmit = (e?: FormEvent) => {
    e?.preventDefault();
    if (newColumnName.trim()) {
      createColumnMutation.mutate({ name: newColumnName.trim() }, {
        onSuccess: () => {
          setNewColumnName('');
          setIsAddingColumn(false);
        }
      });
    } else {
      setIsAddingColumn(false);
    }
  };

  const handleAddColumnKeyDown = (e: KeyboardEvent) => {
    if (e.key === 'Escape') {
      setIsAddingColumn(false);
      setNewColumnName('');
    }
  };
  const handleAddTask = (columnId: string) => {
    setTaskModalMode('create');
    setTaskModalColumnId(columnId);
    setTaskModalTask(null);
    setIsTaskModalOpen(true);
  };

  const handleEditTask = (task: TaskDto) => {
    setTaskModalMode('edit');
    setTaskModalColumnId(task.columnId);
    setTaskModalTask(task);
    setIsTaskModalOpen(true);
  };

  const onDragStart = (event: DragStartEvent) => {
    const { active } = event;
    if (active.data.current?.type === 'Column') {
      setActiveColumn(active.data.current.column);
      return;
    }
    if (active.data.current?.type === 'Task') {
      setActiveTask(active.data.current.task);
      return;
    }
  };

  const onDragOver = (event: DragOverEvent) => {
    const { active, over } = event;
    if (!over) return;

    const activeId = active.id;
    const overId = over.id;

    if (activeId === overId) return;

    const isActiveTask = active.data.current?.type === 'Task';
    const isOverTask = over.data.current?.type === 'Task';
    const isOverColumn = over.data.current?.type === 'Column';

    if (!isActiveTask) return;

    // Moving a Task over another Task
    if (isActiveTask && isOverTask) {
      setTasks(prevTasks => {
        const activeIndex = prevTasks.findIndex(t => t.id === activeId);
        const overIndex = prevTasks.findIndex(t => t.id === overId);

        if (activeIndex < 0 || overIndex < 0) return prevTasks;

        if (prevTasks[activeIndex].columnId !== prevTasks[overIndex].columnId) {
          // Moved to a different column
          const updatedTasks = prevTasks.map((t, idx) =>
            idx === activeIndex
              ? { ...t, columnId: prevTasks[overIndex].columnId }
              : t
          );
          return arrayMove(updatedTasks, activeIndex, overIndex);
        }

        // Reordering within the same column
        return arrayMove(prevTasks, activeIndex, overIndex);
      });
    }

    // Moving a Task to an empty Column space
    if (isActiveTask && isOverColumn) {
      setTasks(prevTasks => {
        const activeIndex = prevTasks.findIndex(t => t.id === activeId);
        if (activeIndex < 0) return prevTasks;

        const updatedTasks = prevTasks.map((t, idx) =>
          idx === activeIndex
            ? { ...t, columnId: overId as string }
            : t
        );

        return arrayMove(updatedTasks, activeIndex, updatedTasks.length - 1);
      });
    }
  };

  const onDragEnd = (event: DragEndEvent) => {
    setActiveColumn(null);
    setActiveTask(null);

    const { active, over } = event;
    if (!over) return;

    const activeId = active.id;
    const overId = over.id;

    if (activeId === overId) return;

    const isActiveColumn = active.data.current?.type === 'Column';
    if (isActiveColumn) {
      setColumns(prevCols => {
        const activeColumnIndex = prevCols.findIndex(col => col.id === activeId);
        const overColumnIndex = prevCols.findIndex(col => col.id === overId);

        if (activeColumnIndex < 0 || overColumnIndex < 0) {
          return prevCols;
        }

        const newCols = (arrayMove(prevCols, activeColumnIndex, overColumnIndex) as ColumnDto[])
          .map((col, index) => ({ ...col, position: (index + 1) * 1000 }));

        const targetIndex = newCols.findIndex(col => col.id === activeId) + 1;
        moveColumnMutation.mutate({ id: activeId as string, newIndex: targetIndex });

        return newCols;
      });
      return;
    }

    const isActiveTask = active.data.current?.type === 'Task';
    if (isActiveTask) {
      const overColumnId = over.data.current?.type === 'Column'
        ? over.id
        : over.data.current?.task?.columnId;

      if (!overColumnId) return;

      setTasks(prevTasks => {
        const activeTask = prevTasks.find(t => t.id === activeId);
        if (!activeTask) {
          return prevTasks;
        }

        const movedTasks = prevTasks.map(task =>
          task.id === activeId
            ? { ...task, columnId: overColumnId as string }
            : task
        );

        const finalTasks = normalizeTaskPositions(movedTasks);
        const targetColumnTasks = finalTasks.filter(t => t.columnId === overColumnId);
        const targetIndex = targetColumnTasks.findIndex(t => t.id === activeId) + 1;

        if (targetIndex <= 0) {
          return prevTasks;
        }

        moveTaskMutation.mutate({
          id: activeId as string,
          targetColumnId: overColumnId as string,
          newIndex: targetIndex,
        });

        return finalTasks;
      });
    }
  };

  if (!board.columns) {
    return (
      <div className="flex min-h-72 items-center justify-center rounded-xl border-2 border-dashed border-indigo-300 bg-white text-sm font-semibold text-indigo-500">
        Loading board...
      </div>
    );
  }

  return (
    <div className="w-full min-w-0 overflow-x-auto overflow-y-hidden pb-4">
      <DndContext
        sensors={sensors}
        collisionDetection={closestCorners}
        onDragStart={onDragStart}
        onDragOver={onDragOver}
        onDragEnd={onDragEnd}
      >
        <div className="flex w-max items-start gap-6 px-2">
          <SortableContext items={columnIds} strategy={horizontalListSortingStrategy}>
            {columns.map((col) => (
              <ColumnContainer
                key={col.id}
                column={col}
                tasks={tasks.filter((task) => task.columnId === col.id)}
                onAddTask={handleAddTask}
                onEditTask={handleEditTask}
              />
            ))}
          </SortableContext>

          {isAddingColumn ? (
            <div className="w-[320px] shrink-0 bg-white rounded-2xl border border-slate-300 p-3 shadow-sm h-min">
              <form onSubmit={handleAddColumnSubmit} className="flex flex-col gap-2">
                <input
                  ref={addColumnInputRef}
                  type="text"
                  placeholder="Enter section name..."
                  value={newColumnName}
                  onChange={(e) => setNewColumnName(e.target.value)}
                  onKeyDown={handleAddColumnKeyDown}
                  className="w-full bg-slate-50 border border-indigo-500 rounded-lg px-3 py-2 text-sm font-semibold text-slate-800 shadow-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/20"
                  disabled={createColumnMutation.isPending}
                />
                <div className="flex items-center gap-2 justify-end">
                  <button
                    type="button"
                    onClick={() => setIsAddingColumn(false)}
                    className="p-1.5 text-slate-400 hover:text-slate-600 hover:bg-slate-100 rounded-lg transition-colors"
                  >
                    <X className="w-4 h-4" />
                  </button>
                  <button
                    type="submit"
                    disabled={createColumnMutation.isPending || !newColumnName.trim()}
                    className="px-3 py-1.5 text-sm font-semibold text-white bg-indigo-600 rounded-lg shadow-sm hover:bg-indigo-700 transition-colors focus:ring-2 focus:ring-indigo-500 focus:outline-none focus:ring-offset-1 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    Add
                  </button>
                </div>
              </form>
            </div>
          ) : (
            <button
              onClick={() => setIsAddingColumn(true)}
              className="h-25 w-[320px] shrink-0 bg-white hover:bg-indigo-50 rounded-2xl p-4 border-2 border-dashed border-indigo-300 hover:border-indigo-500 transition-all flex items-center justify-center gap-2 text-indigo-600 font-semibold text-sm hover:text-indigo-800 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
            >
              <Plus className="w-4 h-4" />
              Add Section
            </button>
          )}
        </div>

        <DragOverlay>
          {activeColumn && (
            <ColumnContainer
              column={activeColumn}
              tasks={tasks.filter((task) => task.columnId === activeColumn.id)}
              isOverlay
            />
          )}
          {activeTask && <TaskCard task={activeTask} isOverlay />}
        </DragOverlay>
      </DndContext>

      <TaskModal
        isOpen={isTaskModalOpen}
        onClose={() => setIsTaskModalOpen(false)}
        boardId={board.id}
        mode={taskModalMode}
        columnId={taskModalColumnId}
        task={taskModalTask}
      />
    </div>
  );
}
