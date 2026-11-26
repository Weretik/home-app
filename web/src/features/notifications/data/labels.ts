import type { Audience, NotificationScheduleItem } from './notifications-api';

/** The chats in the order the page lists them. */
export const audiences: readonly Audience[] = ['Vitaliy', 'Maria', 'Family'];

export const audienceLabels: Record<Audience, string> = {
  Vitaliy: 'Виталий',
  Maria: 'Маша',
  Family: 'Семейная группа',
};

/** A delivery's kind as the schedule names it («Утренние дела»), else the kind itself. */
export const deliveryTitle = (
  kind: string,
  schedule: readonly NotificationScheduleItem[],
) => schedule.find((item) => item.kind === kind)?.title ?? kind;

const kyivTime = new Intl.DateTimeFormat('ru-RU', {
  timeZone: 'Europe/Kyiv',
  day: '2-digit',
  month: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
});

/** An instant from the API as Kyiv time: «27.11, 08:15». */
export const formatKyivTime = (instant: string) =>
  kyivTime.format(new Date(instant));
