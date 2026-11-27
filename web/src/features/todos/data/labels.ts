import type { Assignee, Author, TodoPriority, TodoStatus } from './todos-api';
import { locale } from '../../../platform/locale';

/** Ukrainian names of the contract's enums, in the order the screens list them. */
export const assigneeLabels: Record<Assignee, string> = {
  Maria: 'Маша',
  Vitaliy: 'Віталій',
  Shared: 'Спільне',
};

/** The assignee's avatar letter; Shared shows a two-people icon instead. */
export const assigneeInitials: Record<Assignee, string> = {
  Maria: 'М',
  Vitaliy: 'В',
  Shared: '',
};

export const authorLabels: Record<Author, string> = {
  Maria: 'Маша',
  Vitaliy: 'Віталій',
};

export const statusLabels: Record<TodoStatus, string> = {
  Todo: 'Чекає',
  InProgress: 'В роботі',
  Paused: 'Відкладено',
  Done: 'Зроблено',
  Cancelled: 'Неактуально',
};

/** The statuses of the active list (R-004); the other two are archived. */
export const activeStatuses: readonly TodoStatus[] = [
  'Todo',
  'InProgress',
  'Paused',
];

export const priorityLabels: Record<TodoPriority, string> = {
  Urgent: 'Терміново',
  High: 'Важливо',
  Normal: 'Звичайна',
  Low: 'Колись',
};

/** The mark a row shows before the date; none for Звичайна (FR-004). */
export const priorityMarks: Record<TodoPriority, string> = {
  Urgent: '🔥',
  High: '⭐',
  Normal: '',
  Low: '💭',
};

/** «1 справа», «3 справи», «12 справ». */
export function formatTaskCount(count: number): string {
  const forms = { one: 'справа', few: 'справи', many: 'справ' } as const;
  const rule = new Intl.PluralRules(locale).select(count);
  return `${count} ${forms[rule as keyof typeof forms] ?? forms.many}`;
}
