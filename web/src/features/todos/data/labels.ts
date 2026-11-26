import type { Assignee, Author, TodoPriority, TodoStatus } from './todos-api';

/** Russian names of the contract's enums, in the order the screens list them. */
export const assigneeLabels: Record<Assignee, string> = {
  Maria: 'Маша',
  Vitaliy: 'Виталий',
  Shared: 'Общее',
};

/** The assignee's avatar letter; Shared shows a two-people icon instead. */
export const assigneeInitials: Record<Assignee, string> = {
  Maria: 'М',
  Vitaliy: 'В',
  Shared: '',
};

export const authorLabels: Record<Author, string> = {
  Maria: 'Маша',
  Vitaliy: 'Виталий',
};

export const statusLabels: Record<TodoStatus, string> = {
  Todo: 'Ждёт',
  InProgress: 'В работе',
  Paused: 'Отложено',
  Done: 'Сделано',
  Cancelled: 'Не актуально',
};

/** The statuses of the active list (R-004); the other two are archived. */
export const activeStatuses: readonly TodoStatus[] = [
  'Todo',
  'InProgress',
  'Paused',
];

export const priorityLabels: Record<TodoPriority, string> = {
  Urgent: 'Срочно',
  High: 'Важно',
  Normal: 'Обычно',
  Low: 'Когда-нибудь',
};

/** The mark a row shows before the date; none for Обычно (FR-004). */
export const priorityMarks: Record<TodoPriority, string> = {
  Urgent: '🔥',
  High: '⭐',
  Normal: '',
  Low: '💭',
};

/** «1 задача», «3 задачи», «12 задач». */
export function formatTaskCount(count: number): string {
  const forms = { one: 'задача', few: 'задачи', many: 'задач' } as const;
  const rule = new Intl.PluralRules('ru-RU').select(count);
  return `${count} ${forms[rule as keyof typeof forms] ?? forms.many}`;
}
