import { describe, expect, it } from 'vitest';
import { audienceLabels, deliveryTitle, formatKyivTime } from './labels';

describe('audienceLabels', () => {
  it('names every chat in Russian', () => {
    expect(audienceLabels).toEqual({
      Vitaliy: 'Виталий',
      Maria: 'Маша',
      Family: 'Семейная группа',
    });
  });
});

describe('deliveryTitle', () => {
  const schedule = [
    {
      kind: 'TodosMorning',
      title: 'Утренние дела',
      when: 'каждый день в 8:00',
    },
  ];

  it('takes the title from the schedule, else the kind', () => {
    expect(deliveryTitle('TodosMorning', schedule)).toBe('Утренние дела');
    expect(deliveryTitle('test', schedule)).toBe('test');
  });
});

describe('formatKyivTime', () => {
  it('shows the Kyiv day and time', () => {
    expect(formatKyivTime('2026-11-27T06:15:00Z')).toBe('27.11, 08:15');
  });
});
