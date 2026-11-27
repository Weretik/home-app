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
  expect(statusLabels.Done).toBe('Зроблено');
  expect(statusLabels.Cancelled).toBe('Неактуально');
});

test('names the people and priorities in Ukrainian', () => {
  expect(assigneeLabels).toEqual({
    Maria: 'Маша',
    Vitaliy: 'Віталій',
    Shared: 'Спільне',
  });
  expect(priorityLabels.Low).toBe('Колись');
});

test('Звичайна has no mark, the others do (FR-004)', () => {
  expect(priorityMarks.Normal).toBe('');
  expect(priorityMarks.Urgent).toBe('🔥');
  expect(priorityMarks.High).toBe('⭐');
  expect(priorityMarks.Low).toBe('💭');
});

test('counts tasks with Ukrainian plurals', () => {
  expect(formatTaskCount(1)).toBe('1 справа');
  expect(formatTaskCount(3)).toBe('3 справи');
  expect(formatTaskCount(5)).toBe('5 справ');
  expect(formatTaskCount(21)).toBe('21 справа');
  expect(formatTaskCount(0)).toBe('0 справ');
});
