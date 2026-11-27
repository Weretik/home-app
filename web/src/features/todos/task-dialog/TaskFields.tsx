import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import { categoryLabel, useCategories } from '../data/categories-api';
import {
  assigneeLabels,
  authorLabels,
  priorityLabels,
  priorityMarks,
  statusLabels,
} from '../data/labels';
import type {
  Assignee,
  Author,
  TodoPriority,
  TodoStatus,
} from '../data/todos-api';
import { DueDateField } from './DueDateField';
import { SegmentedField } from './SegmentedField';
import type { FieldErrors, TaskFormValues } from './task-form';

const options = <T extends string>(labels: Record<T, string>) =>
  (Object.keys(labels) as T[]).map((value) => ({
    value,
    label: labels[value],
  }));

const assignees = options<Assignee>(assigneeLabels);
const authors = [
  ...options<Author>(authorLabels),
  { value: '' as const, label: 'не вказано' },
];
const priorities = (['Urgent', 'High', 'Normal', 'Low'] as TodoPriority[]).map(
  (value) => ({
    value,
    label: priorityLabels[value],
    mark: priorityMarks[value] || undefined,
  }),
);

const message = (errors: FieldErrors, name: string) => ({
  error: Boolean(errors[name]),
  helperText: errors[name],
});

interface TaskFieldsProps {
  values: TaskFormValues;
  errors: FieldErrors;
  set: <K extends keyof TaskFormValues>(
    name: K,
    value: TaskFormValues[K],
  ) => void;
}

/** The fields of the task form, one per row (FR-005); the steps follow them in the dialog. */
export function TaskFields({ values, errors, set }: TaskFieldsProps) {
  const categories = useCategories();
  const known =
    values.categoryId === '' ||
    (categories.data ?? []).some((c) => c.id === values.categoryId);

  return (
    <>
      <TextField
        label="Назва"
        required
        multiline
        value={values.title}
        onChange={(event) => set('title', event.target.value)}
        {...message(errors, 'title')}
      />
      <SegmentedField
        label="Кому"
        value={values.assignee}
        options={assignees}
        onChange={(value) => set('assignee', value)}
      />
      <SegmentedField
        label="Поставив(-ла)"
        value={values.author}
        options={authors}
        onChange={(value) => set('author', value)}
      />
      <TextField
        select
        label="Статус"
        value={values.status}
        onChange={(event) => set('status', event.target.value as TodoStatus)}
        {...message(errors, 'status')}
      >
        {options<TodoStatus>(statusLabels).map((option) => (
          <MenuItem key={option.value} value={option.value}>
            {option.label}
          </MenuItem>
        ))}
      </TextField>
      <SegmentedField
        label="Пріоритет"
        value={values.priority}
        options={priorities}
        onChange={(value) => set('priority', value)}
      />
      <DueDateField
        value={values.dueDate}
        error={errors.dueDate}
        onChange={(value) => set('dueDate', value)}
      />
      <TextField
        select
        label="Категорія"
        value={known ? values.categoryId : ''}
        disabled={categories.isPending}
        onChange={(event) => set('categoryId', event.target.value)}
        {...message(errors, 'categoryId')}
        slotProps={{
          select: { displayEmpty: true },
          inputLabel: { shrink: true },
        }}
      >
        <MenuItem value="">Без категорії</MenuItem>
        {(categories.data ?? []).map((category) => (
          <MenuItem key={category.id} value={category.id}>
            {categoryLabel(category)}
          </MenuItem>
        ))}
      </TextField>
      <TextField
        label="Нотатка"
        multiline
        minRows={2}
        value={values.notes}
        onChange={(event) => set('notes', event.target.value)}
        {...message(errors, 'notes')}
      />
    </>
  );
}
