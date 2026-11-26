import type { Assignee, TodoPriority } from './todos-summary-api';

/** Russian names worded like the Todos section (features never import each other, so they are repeated here). */
export const assigneeLabels: Record<Assignee, string> = {
  Maria: 'Маша',
  Vitaliy: 'Виталий',
  Shared: 'Общее',
};

export const assigneeEmoji: Record<Assignee, string> = {
  Maria: '👩',
  Vitaliy: '👨',
  Shared: '👫',
};

export const priorityMarks: Record<TodoPriority, string> = {
  Urgent: '🔥',
  High: '⭐',
  Normal: '',
  Low: '💭',
};

export const priorityLabels: Record<TodoPriority, string> = {
  Urgent: 'Срочно',
  High: 'Важно',
  Normal: 'Обычно',
  Low: 'Когда-нибудь',
};

const weekdays = ['вс', 'пн', 'вт', 'ср', 'чт', 'пт', 'сб'] as const;

const utc = (day: string) => {
  const [year = 2000, month = 1, date = 1] = day.split('-').map(Number);
  return new Date(Date.UTC(year, month - 1, date));
};

const shift = (day: string, days: number) => {
  const date = utc(day);
  date.setUTCDate(date.getUTCDate() + days);
  return date.toISOString().slice(0, 10);
};

/** «сегодня», «завтра», «вчера», else «пн 30.11» (with the year outside this one). `today` is the summary's. */
export function formatDue(day: string, today: string): string {
  if (day === today) return 'сегодня';
  if (day === shift(today, 1)) return 'завтра';
  if (day === shift(today, -1)) return 'вчера';
  const [year, month, date] = day.split('-');
  const suffix = year === today.slice(0, 4) ? '' : `.${year}`;
  return `${weekdays[utc(day).getUTCDay()]} ${date}.${month}${suffix}`;
}
