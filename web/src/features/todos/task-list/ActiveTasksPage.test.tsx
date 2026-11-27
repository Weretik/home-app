import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/query-client';
import { dayHeading } from '../data/day-groups';
import { addDays, kyivToday } from '../data/due-date';
import { TodosSection } from '../section/TodosSection';
import { ActiveTasksPage } from './ActiveTasksPage';

afterEach(() => vi.unstubAllGlobals());

const today = kyivToday();

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
  path = '/todos',
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

test('groups the active tasks by due date and hides empty groups', async () => {
  const { lists } = renderPage(() =>
    json(
      page([
        task({
          title: 'Оплатить свет',
          dueDate: addDays(today, -2),
          priority: 'Urgent',
        }),
        task({
          title: 'Позвонить маме',
          dueDate: today,
          assignee: 'Maria',
          status: 'InProgress',
        }),
        task({
          title: 'Приклеить деревяшки',
          dueDate: addDays(today, 3),
          categoryId: 'c-home',
          categoryName: 'Дом',
          categoryEmoji: '🏠',
          stepsDone: 2,
          stepsTotal: 3,
          notes: 'Клей в гараже',
        }),
        task({ title: 'Разобрать балкон' }),
      ]),
    ),
  );

  const overdue = await screen.findByRole('list', { name: 'Прострочено' });
  expect(within(overdue).getByText('Оплатить свет')).toBeInTheDocument();
  expect(
    within(overdue).getByRole('img', { name: 'Терміново' }),
  ).toBeInTheDocument();
  expect(within(overdue).getByText(/прострочено:/).parentElement).toHaveStyle({
    color: theme.colorSchemes.light!.palette.error.main,
  });
  const todayList = screen.getByRole('list', { name: 'Сьогодні' });
  expect(todayList).toHaveTextContent('сьогодні');
  expect(todayList).toHaveTextContent('В роботі');
  expect(
    within(todayList).getByRole('img', { name: 'Кому: Маша' }),
  ).toBeInTheDocument();
  const soon = screen.getByRole('list', {
    name: dayHeading(addDays(today, 3), today),
  });
  expect(soon).toHaveTextContent('🏠 Дом');
  expect(within(soon).getByLabelText('Кроки: 2 з 3')).toHaveTextContent('2/3');
  expect(within(soon).getByTitle('Є нотатка')).toBeInTheDocument();
  expect(screen.getByRole('list', { name: 'Без терміну' })).toBeInTheDocument();

  const query = lists()[0]!.url.searchParams;
  expect(query.get('view')).toBe('active');
  expect(query.get('pageSize')).toBe('50');
});

test('keeps the filters in the URL and sends them to the API', async () => {
  const { lists } = renderPage(
    () => json(page([task()])),
    '/todos?category=none',
  );
  await screen.findByRole('list', { name: 'Без терміну' });
  expect(lists()[0]!.url.searchParams.get('withoutCategory')).toBe('true');

  await userEvent.click(screen.getByRole('button', { name: 'Маша' }));
  await waitFor(() =>
    expect(lists().at(-1)!.url.searchParams.getAll('assignee')).toEqual([
      'Maria',
      'Shared',
    ]),
  );
  expect(screen.getByTestId('location').textContent).toContain('who=maria');

  await userEvent.type(
    screen.getByRole('searchbox', { name: 'Пошук' }),
    'балкон',
  );
  await waitFor(
    () => expect(lists().at(-1)!.url.searchParams.get('search')).toBe('балкон'),
    {
      timeout: 2000,
    },
  );
  expect(screen.getByTestId('location').textContent).toContain('q=');
});

test('filters by due date from the URL and from «Термін»', async () => {
  const { lists } = renderPage(
    () => json(page([task()])),
    '/todos?when=period&from=2026-12-01&to=2026-12-07',
  );
  await screen.findByRole('list', { name: 'Без терміну' });
  expect(lists()[0]!.url.searchParams.get('dueFrom')).toBe('2026-12-01');
  expect(lists()[0]!.url.searchParams.get('dueTo')).toBe('2026-12-07');
  expect(screen.getByLabelText('З')).toHaveValue('2026-12-01');

  await userEvent.click(screen.getByRole('combobox', { name: 'Термін' }));
  await userEvent.click(screen.getByRole('option', { name: 'Без терміну' }));

  await waitFor(() =>
    expect(lists().at(-1)!.url.searchParams.get('withoutDueDate')).toBe('true'),
  );
  expect(lists().at(-1)!.url.searchParams.get('dueFrom')).toBeNull();
  expect(screen.getByTestId('location').textContent).not.toContain('from=');
  expect(screen.queryByLabelText('З')).not.toBeInTheDocument();
});

test('quick-adds a task for the chosen person and keeps the field ready', async () => {
  const { writes, lists } = renderPage(
    (s) =>
      s.method === 'POST'
        ? json({ id: crypto.randomUUID() }, 201)
        : json(page([task()])),
    '/todos?who=vitaliy',
  );
  await screen.findByRole('list', { name: 'Без терміну' });

  const field = screen.getByRole('textbox', { name: 'Нова справа' });
  await userEvent.type(field, 'Купить хлеб{Enter}');
  await waitFor(() => expect(field).toHaveValue(''));
  expect(field).toHaveFocus();
  await userEvent.type(field, 'Купить хлеб{Enter}');
  await waitFor(() => expect(writes()).toHaveLength(2));

  const [first, second] = writes();
  expect(first!.url.pathname).toBe('/api/todos/tasks');
  expect(first!.body).toMatchObject({
    title: 'Купить хлеб',
    assignee: 'Vitaliy',
    priority: 'Normal',
    dueDate: null,
    steps: [],
  });
  expect(first!.headers.get('Idempotency-Key')).toBeTruthy();
  // The same title on purpose is a second task, so a new key.
  expect(second!.headers.get('Idempotency-Key')).not.toBe(
    first!.headers.get('Idempotency-Key'),
  );
  await waitFor(() => expect(lists().length).toBeGreaterThan(1));
});

test('ticks a task done at once and puts it back with «Скасувати»', async () => {
  const item = task({
    title: 'Разобрать балкон',
    status: 'InProgress',
    version: 5,
  });
  let done = false;
  const { writes } = renderPage((s) => {
    if (s.method === 'PUT') {
      done = (s.body as { status: string }).status === 'Done';
      return noContent();
    }
    if (s.url.pathname === `/api/todos/tasks/${item.id}`)
      return json({ ...item, version: 6 });
    return json(page(done ? [] : [item]));
  });
  await screen.findByRole('list', { name: 'Без терміну' });

  await userEvent.click(
    screen.getByRole('checkbox', { name: 'Готово: Разобрать балкон' }),
  );

  expect(
    screen.queryByText('Разобрать балкон', { selector: 'span' }),
  ).not.toBeInTheDocument();
  const snackbar = await screen.findByText('Готово: Разобрать балкон');
  await waitFor(() => expect(writes()).toHaveLength(1));
  expect(writes()[0]!.url.pathname).toBe(`/api/todos/tasks/${item.id}/status`);
  expect(writes()[0]!.headers.get('If-Match')).toBe('5');
  expect(writes()[0]!.body).toEqual({ status: 'Done' });

  await userEvent.click(
    within(snackbar.closest('[role="alert"]')!).getByRole('button', {
      name: 'Скасувати',
    }),
  );

  await waitFor(() => expect(writes()).toHaveLength(2));
  expect(writes()[1]!.headers.get('If-Match')).toBe('6');
  expect(writes()[1]!.body).toEqual({ status: 'InProgress' });
  expect(
    await screen.findByRole('list', { name: 'Без терміну' }),
  ).toHaveTextContent('Разобрать балкон');
});

test('«Скасувати» waits for the tick to be saved before it restores the status', async () => {
  const item = task({ title: 'Полить цветы', version: 2 });
  let release: () => void = () => undefined;
  let reads = 0;
  const { writes } = renderPage((s) => {
    if (s.method === 'PUT' && (s.body as { status: string }).status === 'Done')
      return new Promise<Response>((resolve) => {
        release = () => resolve(new Response(null, { status: 204 }));
      });
    if (s.method === 'PUT') return noContent();
    if (s.url.pathname === `/api/todos/tasks/${item.id}`) {
      reads += 1;
      return json({ ...item, status: 'Done', version: 3 });
    }
    return json(page([item]));
  });
  await screen.findByRole('list', { name: 'Без терміну' });

  await userEvent.click(
    screen.getByRole('checkbox', { name: 'Готово: Полить цветы' }),
  );
  const snackbar = await screen.findByText('Готово: Полить цветы');
  await userEvent.click(
    within(snackbar.closest('[role="alert"]')!).getByRole('button', {
      name: 'Скасувати',
    }),
  );

  // The tick is still in flight: the undo neither reads the task nor writes yet.
  expect(reads).toBe(0);
  expect(writes()).toHaveLength(1);

  release();

  await waitFor(() => expect(writes()).toHaveLength(2));
  expect(writes()[1]!.headers.get('If-Match')).toBe('3');
  expect(writes()[1]!.body).toEqual({ status: 'Todo' });
});

test('brings a ticked task back and says why when it changed elsewhere', async () => {
  renderPage((s) =>
    s.method === 'PUT'
      ? json(
          {
            title: 'Conflict',
            detail: 'Changed.',
            code: 'todos.changed_meanwhile',
          },
          409,
        )
      : json(page([task({ title: 'Оплатить свет' })])),
  );
  await screen.findByRole('list', { name: 'Без терміну' });

  await userEvent.click(
    screen.getByRole('checkbox', { name: 'Готово: Оплатить свет' }),
  );

  expect(
    await screen.findByText(/Справу змінили в іншому місці/),
  ).toBeInTheDocument();
  expect(
    await screen.findByRole('list', { name: 'Без терміну' }),
  ).toHaveTextContent('Оплатить свет');
});

test('says the list is empty, offers to reset a filter and retries an error', async () => {
  let fail = true;
  renderPage(
    () => (fail ? json({ title: 'Ошибка сервера' }, 500) : json(page([]))),
    '/todos?who=shared',
  );

  expect(await screen.findByRole('alert')).toHaveTextContent(
    'Не вдалося завантажити',
  );
  fail = false;
  await userEvent.click(screen.getByRole('button', { name: 'Повторити' }));

  expect(
    await screen.findByText('З цим фільтром справ немає'),
  ).toBeInTheDocument();
  await userEvent.click(screen.getByRole('button', { name: 'Скинути фільтр' }));
  expect(await screen.findByText('Справ немає')).toBeInTheDocument();
  expect(screen.getByText('Додайте першу вище.')).toBeInTheDocument();
});
