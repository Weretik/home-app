import type {
  Assignee,
  Author,
  TodoPriority,
  TodoStatus,
  TodoTaskItem,
} from '../data/todos-api';
import type {
  TodoTaskRequest,
  TodoTaskUpdate,
} from '../writes/task-writes-api';

/** The API limits (R-001, R-002, R-007). */
export const MAX_TITLE = 200;
export const MAX_NOTES = 4000;
const FIRST_DAY = '2000-01-01';
const LAST_DAY = '2100-12-31';

/** What the task form edits; '' is «none» for the optional choices. */
export interface TaskFormValues {
  title: string;
  assignee: Assignee;
  author: Author | '';
  status: TodoStatus;
  priority: TodoPriority;
  dueDate: string;
  categoryId: string;
  notes: string;
}

/** Field messages by the contract's camelCase field name. */
export type FieldErrors = Record<string, string | undefined>;

/** The form of an existing task, or of a new one for `assignee` (the list's «Кому»). */
export function taskFormValues(
  item: TodoTaskItem | undefined,
  assignee: Assignee = 'Shared',
): TaskFormValues {
  if (!item)
    return {
      title: '',
      assignee,
      author: '',
      status: 'Todo',
      priority: 'Normal',
      dueDate: '',
      categoryId: '',
      notes: '',
    };
  return {
    title: item.title,
    assignee: item.assignee,
    author: item.author ?? '',
    status: item.status,
    priority: item.priority,
    dueDate: item.dueDate ?? '',
    categoryId: item.categoryId ?? '',
    notes: item.notes ?? '',
  };
}

export function titleError(title: string): string | undefined {
  const trimmed = title.trim();
  if (trimmed === '') return 'Введите название';
  if (trimmed.length > MAX_TITLE)
    return `Название не длиннее ${MAX_TITLE} символов`;
  return undefined;
}

/** The form's own checks, before anything is sent (FR-005). */
export function validateTask(values: TaskFormValues): FieldErrors {
  const errors: FieldErrors = { title: titleError(values.title) };
  if (values.notes.trim().length > MAX_NOTES)
    errors.notes = `Заметка не длиннее ${MAX_NOTES} символов`;
  if (
    values.dueDate &&
    (!/^\d{4}-\d{2}-\d{2}$/.test(values.dueDate) ||
      values.dueDate < FIRST_DAY ||
      values.dueDate > LAST_DAY)
  )
    errors.dueDate = 'Дата между 2000 и 2100 годом';
  return errors;
}

export const isValid = (errors: FieldErrors) =>
  Object.values(errors).every((message) => !message);

/** The edit request of the values: trimmed text, '' sent as null. */
export function toTaskUpdate(values: TaskFormValues): TodoTaskUpdate {
  const notes = values.notes.trim();
  return {
    title: values.title.trim(),
    notes: notes === '' ? null : notes,
    assignee: values.assignee,
    author: values.author === '' ? null : values.author,
    status: values.status,
    priority: values.priority,
    dueDate: values.dueDate === '' ? null : values.dueDate,
    categoryId: values.categoryId === '' ? null : values.categoryId,
  };
}

/** The create request: the values and the titles of the steps added before the first save. */
export function toTaskCreate(
  values: TaskFormValues,
  steps: readonly string[],
): TodoTaskRequest {
  return { ...toTaskUpdate(values), steps: [...steps] };
}

/** The create request of a quick-added title (FR-003): Обычно, no date, no category. */
export function quickTask(title: string, assignee: Assignee): TodoTaskRequest {
  return toTaskCreate({ ...taskFormValues(undefined, assignee), title }, []);
}
