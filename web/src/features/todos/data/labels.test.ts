import {
  activeStatuses,
  assigneeLabels,
  formatTaskCount,
  priorityLabels,
  priorityMarks,
  statusLabels,
} from './labels';

test('the active statuses are the three that are not archived (R-004)', () => {
  expect(activeStatuses).toEqual(['Todo', 'InProgress', 'Paused']);
  expect(statusLabels.Done).toBe('Сделано');
  expect(statusLabels.Cancelled).toBe('Не актуально');
});

test('names the people and priorities in Russian', () => {
  expect(assigneeLabels).toEqual({
    Maria: 'Маша',
    Vitaliy: 'Виталий',
    Shared: 'Общее',
  });
  expect(priorityLabels.Low).toBe('Когда-нибудь');
});

test('Обычно has no mark, the others do (FR-004)', () => {
  expect(priorityMarks.Normal).toBe('');
  expect(priorityMarks.Urgent).toBe('🔥');
  expect(priorityMarks.High).toBe('⭐');
  expect(priorityMarks.Low).toBe('💭');
});

test('counts tasks with Russian plurals', () => {
  expect(formatTaskCount(1)).toBe('1 задача');
  expect(formatTaskCount(3)).toBe('3 задачи');
  expect(formatTaskCount(5)).toBe('5 задач');
  expect(formatTaskCount(21)).toBe('21 задача');
  expect(formatTaskCount(0)).toBe('0 задач');
});
