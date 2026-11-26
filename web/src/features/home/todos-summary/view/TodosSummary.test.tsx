import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../../app/theme/theme';
import { createQueryClient } from '../../../../platform/query-client';
import { HomePage } from '../../page/HomePage';

afterEach(() => vi.unstubAllGlobals());

const task = (overrides: Record<string, unknown> = {}) => ({
  id: crypto.randomUUID(),
  title: 'Приклеить деревяшки',
  notes: null,
  assignee: 'Maria',
  author: 'Vitaliy',
  status: 'Todo',
  priority: 'High',
  dueDate: '2026-11-24',
  categoryId: null,
  categoryName: null,
  categoryEmoji: null,
  createdAtUtc: '2026-11-20T10:00:00Z',
  updatedAtUtc: '2026-11-20T10:00:00Z',
  completedAtUtc: null,
  steps: [],
  stepsDone: 0,
  stepsTotal: 0,
  version: 7,
  ...overrides,
});

const summary = (focus: unknown[]) => ({
  today: '2026-11-26',
  assignees: [
    { assignee: 'Maria', active: 4, overdue: 1, dueToday: 2 },
    { assignee: 'Vitaliy', active: 1, overdue: 0, dueToday: 0 },
    { assignee: 'Shared', active: 3, overdue: 0, dueToday: 1 },
  ],
  doneLast7Days: 5,
  focus,
});

const json = (body: unknown, status = 200) =>
  Promise.resolve(
    new Response(JSON.stringify(body), {
      status,
      headers: { 'Content-Type': 'application/json' },
    }),
  );

interface Sent {
  method: string;
  url: URL;
  headers: Headers;
  body: unknown;
}

function renderHome(answer: (sent: Sent) => Promise<Response>) {
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
    return answer(entry);
  });
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={['/?s=todos']}>
          <HomePage />
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  return sent;
}

const plain = (element: HTMLElement) =>
  element.textContent?.replace(/\s/g, ' ');

test('shows the counts per assignee, the week and the focus list', async () => {
  renderHome(() =>
    json(
      summary([
        task(),
        task({
          title: 'Оплатить свет',
          dueDate: '2026-11-26',
          priority: 'Urgent',
          assignee: 'Shared',
        }),
      ]),
    ),
  );

  const maria = await screen.findByRole('region', { name: 'Маша' });
  expect(plain(maria)).toContain('4 задачи');
  expect(within(maria).getByText('просрочено 1')).toHaveStyle({
    color: theme.colorSchemes.light!.palette.error.main,
  });
  expect(maria).toHaveTextContent('сегодня 2');
  expect(screen.getByRole('region', { name: 'Виталий' })).toHaveTextContent(
    'на сегодня всё спокойно',
  );
  expect(screen.getByText('Сделано за неделю: 5')).toBeInTheDocument();
  expect(screen.getByRole('link', { name: 'Открыть дела' })).toHaveAttribute(
    'href',
    '/todos',
  );

  const rows = within(
    screen.getByRole('list', { name: 'В фокусе' }),
  ).getAllByRole('listitem');
  expect(rows).toHaveLength(2);
  expect(rows[0]).toHaveTextContent('вт 24.11');
  expect(rows[1]).toHaveTextContent('сегодня');
  expect(rows[1]).toHaveTextContent('Общее');
});

test('ticks a focus task done with its version and reloads the summary', async () => {
  const focus = task();
  const sent = renderHome((s) =>
    s.method === 'PUT'
      ? Promise.resolve(new Response(null, { status: 204 }))
      : json(summary([focus])),
  );

  await userEvent.click(
    await screen.findByRole('checkbox', {
      name: 'Готово: Приклеить деревяшки',
    }),
  );

  await waitFor(() =>
    expect(sent.filter((s) => s.method === 'GET')).toHaveLength(2),
  );
  const put = sent.find((s) => s.method === 'PUT')!;
  expect(put.url.pathname).toBe(`/api/todos/tasks/${focus.id}/status`);
  expect(put.headers.get('If-Match')).toBe('7');
  expect(put.body).toEqual({ status: 'Done' });
});

test('says when nothing needs attention', async () => {
  renderHome(() => json(summary([])));

  expect(await screen.findByText(/Ничего срочного/)).toBeInTheDocument();
});

test('says in Russian when a focus task changed elsewhere', async () => {
  renderHome((s) =>
    s.method === 'PUT'
      ? json(
          {
            title: 'Conflict',
            detail: 'Someone changed or deleted this in the meantime.',
            code: 'todos.changed_meanwhile',
          },
          409,
        )
      : json(summary([task()])),
  );

  await userEvent.click(
    await screen.findByRole('checkbox', {
      name: 'Готово: Приклеить деревяшки',
    }),
  );

  expect(
    await screen.findByText(
      'Задачу изменили в другом месте — сводка обновлена.',
    ),
  ).toBeInTheDocument();
});
