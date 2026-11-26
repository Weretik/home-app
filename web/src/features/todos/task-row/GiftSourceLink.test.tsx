import { ThemeProvider } from '@mui/material/styles';
import { render, screen } from '@testing-library/react';
import type { ReactNode } from 'react';
import { MemoryRouter } from 'react-router-dom';
import { theme } from '../../../app/theme/theme';
import type { TodoTaskItem } from '../data/todos-api';
import { GiftSourceLink } from './GiftSourceLink';
import { TaskMeta } from './TaskMeta';

const task = (source: TodoTaskItem['source']): TodoTaskItem => ({
  id: 't1',
  title: '🎁 Купить подарок: Духи',
  notes: null,
  assignee: 'Vitaliy',
  author: null,
  status: 'Todo',
  priority: 'High',
  dueDate: '2026-12-20',
  categoryId: null,
  categoryName: null,
  categoryEmoji: null,
  createdAtUtc: '2026-11-20T10:00:00Z',
  updatedAtUtc: '2026-11-20T10:00:00Z',
  completedAtUtc: null,
  steps: [],
  stepsDone: 0,
  stepsTotal: 0,
  version: 1,
  source,
});

const themed = (node: ReactNode) =>
  render(
    <ThemeProvider theme={theme}>
      <MemoryRouter>{node}</MemoryRouter>
    </ThemeProvider>,
  );

const gift = { kind: 'gift', id: 'g 1', url: 'https://site/gifts?gift=g%201' };

test('a gift to-do links to its gift inside the app', () => {
  themed(<GiftSourceLink task={task(gift)} />);

  expect(
    screen.getByRole('link', { name: '🎁 Открыть подарок' }),
  ).toHaveAttribute('href', '/gifts?gift=g%201');
});

test('other tasks have no gift link and no gift mark', () => {
  themed(
    <>
      <GiftSourceLink task={task(null)} />
      <TaskMeta task={task(null)} today="2026-11-27" />
    </>,
  );

  expect(screen.queryByRole('link')).not.toBeInTheDocument();
  expect(screen.queryByRole('img', { name: 'Задача из подарков' })).toBeNull();
});

test('the meta line marks a gift to-do', () => {
  themed(<TaskMeta task={task(gift)} today="2026-11-27" />);

  expect(
    screen.getByRole('img', { name: 'Задача из подарков' }),
  ).toBeInTheDocument();
});
