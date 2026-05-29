import { useState, useMemo, useEffect } from 'react';
import {
  DndContext,
  DragOverlay,
  closestCorners,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
} from '@dnd-kit/core';
import type { DragStartEvent, DragOverEvent, DragEndEvent } from '@dnd-kit/core';
import { SortableContext, horizontalListSortingStrategy, arrayMove } from '@dnd-kit/sortable';
import type { BoardDetailResponse, ColumnDto } from '../../column/types';
import type { TaskDto } from '../../task/types';
import ColumnContainer from '../../column/components/ColumnContainer';
import TaskCard from '../../task/components/TaskCard';
import { Plus } from 'lucide-react';

interface KanbanBoardProps {
  board: BoardDetailResponse;
}

export default function KanbanBoard({ board }: KanbanBoardProps) {
  // Local state for optimistic drag-and-drop updates
  const [columns, setColumns] = useState<ColumnDto[]>(board.columns || []);
  const [tasks, setTasks] = useState<TaskDto[]>([]);

  const [activeColumn, setActiveColumn] = useState<ColumnDto | null>(null);
  const [activeTask, setActiveTask] = useState<TaskDto | null>(null);

  // Sync state when board prop updates via react-query
  useEffect(() => {
    setColumns(board.columns || []);
    setTasks((board.columns || []).flatMap((col) => col.tasks || []));
  }, [board]);

  const columnIds = useMemo(() => columns.map((col) => col.id), [columns]);

  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 5, // 5px movement threshold before initiating drag
      },
    }),
    useSensor(KeyboardSensor)
  );

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
      setTasks((prevTasks) => {
        const activeIndex = prevTasks.findIndex((t) => t.id === activeId);
        const overIndex = prevTasks.findIndex((t) => t.id === overId);

        if (prevTasks[activeIndex].columnId !== prevTasks[overIndex].columnId) {
          // Moved to a different column
          const updatedTasks = [...prevTasks];
          updatedTasks[activeIndex].columnId = prevTasks[overIndex].columnId;
          return arrayMove(updatedTasks, activeIndex, overIndex);
        }

        // Reordering within the same column
        return arrayMove(prevTasks, activeIndex, overIndex);
      });
    }

    // Moving a Task to an empty Column space
    if (isActiveTask && isOverColumn) {
      setTasks((prevTasks) => {
        const activeIndex = prevTasks.findIndex((t) => t.id === activeId);
        const updatedTasks = [...prevTasks];
        updatedTasks[activeIndex].columnId = overId as string;
        
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

    // Column Reordering
    const isActiveColumn = active.data.current?.type === 'Column';
    if (isActiveColumn) {
      setColumns((prevCols) => {
        const activeColumnIndex = prevCols.findIndex((col) => col.id === activeId);
        const overColumnIndex = prevCols.findIndex((col) => col.id === overId);
        const newCols = arrayMove(prevCols, activeColumnIndex, overColumnIndex);
        
        // TODO: Muation -> trigger backend API update mapping the newly sorted column positions here
        return newCols;
      });
      return;
    }

    // Task Reordering
    const isActiveTask = active.data.current?.type === 'Task';
    if (isActiveTask) {
       // Optimistic positioning is already handled by onDragOver.
       // TODO: Mutation -> trigger backend API to persist task new columnId and sequence positioning
    }
  };

  if (!columns.length) return null;

  return (
    <div className="flex h-full w-full overflow-x-auto overflow-y-hidden pb-4">
      <DndContext
        sensors={sensors}
        collisionDetection={closestCorners}
        onDragStart={onDragStart}
        onDragOver={onDragOver}
        onDragEnd={onDragEnd}
      >
        <div className="flex items-start gap-6 px-2">
          <SortableContext items={columnIds} strategy={horizontalListSortingStrategy}>
            {columns.map((col) => (
              <ColumnContainer
                key={col.id}
                column={col}
                tasks={tasks.filter((task) => task.columnId === col.id)}
              />
            ))}
          </SortableContext>
          
          <button className="w-[320px] shrink-0 bg-slate-100/40 hover:bg-slate-100/80 rounded-2xl p-4 border-2 border-dashed border-slate-200/80 hover:border-indigo-300 transition-all flex items-center justify-center gap-2 text-slate-500 font-medium text-sm h-25 hover:text-indigo-600 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2">
            <Plus className="w-4 h-4" />
            Add Section
          </button>
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
    </div>
  );
}
