import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/client/query-client';
import { TodosSection } from '../section/TodosSection';
import { ActiveTasksPage } from '../task-list/ActiveTasksPage';
import { ArchivePage } from './ArchivePage';

afterEach(() => vi.unstubAllGlobals());

const categories = [
  {
    id: 'c-home',
    name: 'Дом',
    emoji: '🏠',
    sortOrder: 10,
    version: 1,
    activeCount: 2,
  },
];

const task = (overrides: Record<string, unknown> = {}) => ({
  id: crypto.randomUUID(),
  title: 'Задача',
  notes: null,
  assignee: 'Shared',
  author: null,
  status: 'Todo',
  priority: 'Normal',
  dueDate: null,
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
  ...overrides,
});

const page = (items: unknown[]) => ({
  items,
  page: 1,
  pageSize: 50,
  totalCount: items.length,
});

const json = (body: unknown, status = 200) =>
  Promise.resolve(
    new Response(JSON.stringify(body), {
      status,
      headers: { 'Content-Type': 'application/json' },
    }),
  );
const noContent = () => Promise.resolve(new Response(null, { status: 204 }));

interface Sent {
  method: string;
  url: URL;
  headers: Headers;
  body: unknown;
}

function LocationProbe() {
  return <output data-testid="location">{useLocation().search}</output>;
}

/** Stubs the API: categories always answer; `answer` handles the rest. */
function renderPage(
  answer: (sent: Sent) => Promise<Response>,
  path = '/todos/archive',
) {
  const sent: Sent[] = [];
  vi.stubGlobal('fetch', async (request: Request) => {
    const text = await request.text();
    const entry = {
      method: request.method,
      url: new URL(request.url),
      headers: request.headers,
      body: text ? JSON.parse(text) : undefined,
    };
    sent.push(entry);
    return entry.url.pathname === '/api/todos/categories'
      ? json(categories)
      : answer(entry);
  });
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={[path]}>
          <Routes>
            <Route path="/todos" element={<TodosSection />}>
              <Route index element={<ActiveTasksPage />} />
              <Route path="archive" element={<ArchivePage />} />
            </Route>
          </Routes>
          <LocationProbe />
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  const lists = () =>
    sent.filter(
      (s) => s.method === 'GET' && s.url.pathname === '/api/todos/tasks',
    );
  const writes = () => sent.filter((s) => s.method !== 'GET');
  return { sent, lists, writes };
}

const archived = (overrides: Record<string, unknown> = {}) =>
  task({
    status: 'Done',
    completedAtUtc: new Date().toISOString(),
    ...overrides,
  });

test('shows done and cancelled tasks with their completion and pages them', async () => {
  const { lists } = renderPage((s) =>
    s.url.searchParams.get('page') === '1'
      ? json({
          ...page([archived({ title: 'Приклеить деревяшки' })]),
          pageSize: 1,
          totalCount: 2,
        })
      : json({
          ...page([
            archived({
              title: 'Старый план',
              status: 'Cancelled',
              completedAtUtc: '2026-01-05T10:00:00Z',
            }),
          ]),
          page: 2,
          pageSize: 1,
          totalCount: 2,
        }),
  );

  const list = await screen.findByRole('list', { name: 'Сьогодні' });
  expect(list).toHaveTextContent('Зроблено');
  expect(lists()[0]!.url.searchParams.get('view')).toBe('archive');
  expect(screen.getByRole('tab', { name: 'Справи' })).toHaveAttribute(
    'href',
    '/todos',
  );

  await userEvent.click(screen.getByRole('button', { name: 'Показати ще' }));

  expect(await screen.findByText('Старый план')).toBeInTheDocument();
  const older = screen.getByRole('list', { name: /5 січня/ });
  expect(older).toHaveTextContent('Неактуально');
  expect(lists().at(-1)!.url.searchParams.get('page')).toBe('2');
});

test('returns a task to the active list with «Повернути»', async () => {
  const item = archived({ title: 'Приклеить деревяшки', version: 4 });
  let returned = false;
  const { writes } = renderPage((s) => {
    if (s.method === 'PUT') {
      returned = true;
      return noContent();
    }
    return json(page(returned ? [] : [item]));
  }, '/todos/archive?who=maria');
  await screen.findByRole('list', { name: 'Сьогодні' });
  expect(screen.getByRole('tab', { name: 'Справи' })).toHaveAttribute(
    'href',
    '/todos?who=maria',
  );

  await userEvent.click(
    screen.getByRole('button', { name: 'Повернути: Приклеить деревяшки' }),
  );

  expect(
    await screen.findByText('Повернули до справ: Приклеить деревяшки'),
  ).toBeInTheDocument();
  const [put] = writes();
  expect(put!.url.pathname).toBe(`/api/todos/tasks/${item.id}/status`);
  expect(put!.headers.get('If-Match')).toBe('4');
  expect(put!.body).toEqual({ status: 'Todo' });
  expect(
    await screen.findByText('З цим фільтром справ немає'),
  ).toBeInTheDocument();
});

test('says the archive is empty', async () => {
  renderPage(() => json(page([])));

  expect(await screen.findByText('Архів порожній')).toBeInTheDocument();
});

test('filters the archive by the day of completion', async () => {
  const { lists } = renderPage(() =>
    json(page([archived({ title: 'Позвонить' })])),
  );
  await screen.findByRole('list', { name: 'Сьогодні' });

  await userEvent.click(
    screen.getByRole('combobox', { name: 'Коли зроблено' }),
  );
  await userEvent.click(screen.getByRole('option', { name: 'Вчора' }));

  await vi.waitFor(() => {
    const query = lists().at(-1)!.url.searchParams;
    expect(query.get('completedFrom')).toBeTruthy();
    expect(query.get('completedFrom')).toBe(query.get('completedTo'));
    expect(query.get('dueTo')).toBeNull();
  });
});
