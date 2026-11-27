import { describe, expect, it } from 'vitest';
import type { NotificationScheduleItem } from './notifications-api';
import { audienceLabels, deliveryTitle, formatKyivTime } from './labels';

describe('audienceLabels', () => {
  it('names every chat in Russian', () => {
    expect(audienceLabels).toEqual({
      Vitaliy: 'Віталій',
      Maria: 'Маша',
      Family: 'Сімейна група',
    });
  });
});

describe('deliveryTitle', () => {
  const schedule: NotificationScheduleItem[] = [
    {
      kind: 'TodosMorning',
      title: 'Ранкові справи',
      when: 'щодня о 8:00',
    },
  ];

  it('takes the title from the schedule, else the kind', () => {
    expect(deliveryTitle('TodosMorning', schedule)).toBe('Ранкові справи');
    expect(deliveryTitle('ExpensesWeek', schedule)).toBe('ExpensesWeek');
  });
});

describe('formatKyivTime', () => {
  it('shows the Kyiv day and time', () => {
    expect(formatKyivTime('2026-11-27T06:15:00Z')).toBe('27.11, 08:15');
  });
});
