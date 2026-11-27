import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/client/query-client';
import { addDays, kyivToday } from '../data/due-date';
import { TodosSection } from '../section/TodosSection';
import { ActiveTasksPage } from '../task-list/ActiveTasksPage';

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
  steps: [] as unknown[],
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

const step = (title: string, isDone = false) => ({
  id: crypto.randomUUID(),
  title,
  isDone,
});

const openTask = async (title: string) => {
  await userEvent.click(
    await screen.findByRole('button', { name: new RegExp(title) }),
  );
  return screen.findByRole('dialog', { name: 'Справа' });
};

test('saves steps of an existing task at once and the task with its current version', async () => {
  const steps = [
    step('Купить клей', true),
    step('Шлифовать'),
    step('Покрасить'),
  ];
  let current = task({
    title: 'Приклеить деревяшки',
    assignee: 'Maria',
    dueDate: addDays(today, 3),
    steps,
    stepsDone: 1,
    stepsTotal: 3,
    version: 3,
  });
  const { writes } = renderPage((s) => {
    if (s.method === 'GET')
      return s.url.pathname === '/api/todos/tasks'
        ? json(page([current]))
        : json(current);
    if (s.url.pathname.includes('/steps')) {
      // Every step change gives the task a new version (R-010).
      current = { ...current, version: (current.version as number) + 1 };
      if (s.method === 'POST') {
        current = {
          ...current,
          steps: [...(current.steps as unknown[]), step('Лак')],
        };
        return json({ id: crypto.randomUUID() }, 201);
      }
      if (s.method === 'DELETE')
        current = {
          ...current,
          steps: (current.steps as { id: string }[]).filter(
            (x) => !s.url.pathname.endsWith(x.id),
          ),
        };
      return noContent();
    }
    return noContent();
  });
  const dialog = await openTask('Приклеить деревяшки');
  const stepList = within(dialog).getByRole('list', { name: 'Список кроків' });
  expect(within(stepList).getAllByRole('listitem')).toHaveLength(3);

  await userEvent.click(
    within(dialog).getByRole('checkbox', { name: 'Крок виконано: Шлифовать' }),
  );
  await waitFor(() => expect(writes()).toHaveLength(1));
  expect(writes()[0]!.method).toBe('PUT');
  expect(writes()[0]!.url.pathname).toBe(
    `/api/todos/tasks/${current.id}/steps/${steps[1]!.id}`,
  );
  expect(writes()[0]!.body).toEqual({ title: 'Шлифовать', isDone: true });
  expect(writes()[0]!.headers.has('If-Match')).toBe(false);

  await userEvent.type(
    within(dialog).getByRole('textbox', { name: 'Новий крок' }),
    'Лак{Enter}',
  );
  await waitFor(() => expect(writes()).toHaveLength(2));
  expect(writes()[1]!.body).toEqual({ title: 'Лак' });
  expect(
    await within(dialog).findByRole('textbox', { name: 'Крок: Лак' }),
  ).toBeInTheDocument();

  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Видалити крок: Покрасить' }),
  );
  await waitFor(() => expect(writes()).toHaveLength(3));
  expect(writes()[2]!.method).toBe('DELETE');
  await waitFor(() =>
    expect(
      within(dialog).queryByRole('textbox', { name: 'Крок: Покрасить' }),
    ).not.toBeInTheDocument(),
  );

  const title = within(dialog).getByRole('textbox', { name: /Назва/ });
  await userEvent.clear(title);
  await userEvent.type(title, 'Приклеить рейки');
  await waitFor(() =>
    expect(
      within(dialog).getByRole('button', { name: 'Зберегти' }),
    ).toBeEnabled(),
  );
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await waitFor(() =>
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument(),
  );

  const put = writes().at(-1)!;
  expect(put.url.pathname).toBe(`/api/todos/tasks/${current.id}`);
  expect(put.headers.get('If-Match')).toBe('6');
  expect(put.body).toEqual({
    title: 'Приклеить рейки',
    notes: null,
    assignee: 'Maria',
    author: null,
    status: 'Todo',
    priority: 'Normal',
    dueDate: addDays(today, 3),
    categoryId: null,
  });
});

test('reloads the form when the task was changed elsewhere', async () => {
  let version = 1;
  const item = () =>
    task({ title: version === 1 ? 'Старое' : 'Новое от Маши', version });
  renderPage((s) => {
    if (s.method === 'PUT') {
      version = 2;
      return json(
        {
          title: 'Conflict',
          detail: 'Changed.',
          code: 'todos.changed_meanwhile',
        },
        409,
      );
    }
    return s.url.pathname === '/api/todos/tasks'
      ? json(page([item()]))
      : json(item());
  });
  const dialog = await openTask('Старое');

  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );

  expect(await within(dialog).findByRole('alert')).toHaveTextContent(
    'Справу змінили в іншому місці',
  );
  await waitFor(() =>
    expect(within(dialog).getByRole('textbox', { name: /Назва/ })).toHaveValue(
      'Новое от Маши',
    ),
  );
});

test('creates a task from the full form with its steps and quick due date', async () => {
  const { writes } = renderPage(
    (s) =>
      s.method === 'POST'
        ? json({ id: crypto.randomUUID() }, 201)
        : json(page([task()])),
    '/todos?who=maria',
  );
  await screen.findByRole('list', { name: 'Без терміну' });
  await userEvent.type(
    screen.getByRole('textbox', { name: 'Нова справа' }),
    'Приклеить деревяшки',
  );
  await userEvent.click(screen.getByRole('button', { name: 'Детальна форма' }));
  const dialog = await screen.findByRole('dialog', { name: 'Нова справа' });
  expect(within(dialog).getByRole('textbox', { name: /Назва/ })).toHaveValue(
    'Приклеить деревяшки',
  );

  await userEvent.click(
    within(dialog).getByRole('button', { name: /Важливо/ }),
  );
  await userEvent.click(within(dialog).getByRole('button', { name: 'Завтра' }));
  const author = within(dialog).getAllByRole('group', {
    name: 'Поставив(-ла)',
  })[0]!;
  await userEvent.click(
    within(author).getByRole('button', { name: 'Віталій' }),
  );
  await userEvent.click(
    within(dialog).getByRole('combobox', { name: 'Категорія' }),
  );
  await userEvent.click(await screen.findByRole('option', { name: '🏠 Дом' }));
  await userEvent.type(
    within(dialog).getByRole('textbox', { name: 'Новий крок' }),
    'Купить клей{Enter}',
  );
  await userEvent.type(
    within(dialog).getByRole('textbox', { name: 'Новий крок' }),
    'Шлифовать{Enter}',
  );
  expect(
    within(dialog).getByRole('checkbox', {
      name: 'Крок виконано: Купить клей',
    }),
  ).toBeDisabled();
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await waitFor(() =>
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument(),
  );

  const [post] = writes();
  expect(post!.headers.get('Idempotency-Key')).toBeTruthy();
  expect(post!.body).toEqual({
    title: 'Приклеить деревяшки',
    notes: null,
    assignee: 'Maria',
    author: 'Vitaliy',
    status: 'Todo',
    priority: 'High',
    dueDate: addDays(today, 1),
    categoryId: 'c-home',
    steps: ['Купить клей', 'Шлифовать'],
  });
});

test('asks before deleting a task', async () => {
  const item = task({ title: 'Лишняя', version: 9 });
  const { writes } = renderPage((s) =>
    s.method === 'DELETE'
      ? noContent()
      : s.url.pathname === '/api/todos/tasks'
        ? json(page([item]))
        : json(item),
  );
  const dialog = await openTask('Лишняя');

  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Видалити' }),
  );
  const confirm = await screen.findByRole('dialog', {
    name: 'Видалити справу?',
  });
  expect(writes()).toHaveLength(0);
  await userEvent.click(
    within(confirm).getByRole('button', { name: 'Видалити' }),
  );

  await waitFor(() =>
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument(),
  );
  expect(writes()[0]!.method).toBe('DELETE');
  expect(writes()[0]!.headers.get('If-Match')).toBe('9');
});
