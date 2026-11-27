import { periodError } from './period';

test('accepts a period of one day up to 366 days', () => {
  expect(periodError({ from: '2026-10-05', to: '2026-10-05' })).toBeUndefined();
  expect(periodError({ from: '2024-01-01', to: '2024-12-31' })).toBeUndefined();
  expect(periodError({ from: '2026-01-01', to: '2027-01-01' })).toBeUndefined();
});

test('explains why a period cannot be shown', () => {
  expect(periodError({ from: '', to: '2026-10-05' })).toBe(
    'Виберіть обидві дати',
  );
  expect(periodError({ from: '2026-10-05', to: '' })).toBe(
    'Виберіть обидві дати',
  );
  expect(periodError({ from: '2026-10-06', to: '2026-10-05' })).toBe(
    'Дата «з» пізніше за дату «по»',
  );
  expect(periodError({ from: '2026-01-01', to: '2027-01-02' })).toBe(
    'Період не довше 366 днів',
  );
  expect(periodError({ from: '1999-12-31', to: '2000-01-01' })).toBe(
    'Дати між 2000 і 2100 роком',
  );
  expect(periodError({ from: '2026-02-30', to: '2026-03-01' })).toBe(
    'Дати між 2000 і 2100 роком',
  );
});
