import { locale } from '../../../platform/client/locale';
import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/client/query-client';
import { HistoryPage } from './HistoryPage';

afterEach(() => vi.unstubAllGlobals());

const today = new Date();
const yesterday = new Date(today);
yesterday.setDate(today.getDate() - 1);
const at = (day: Date, hour: number) =>
  new Date(
    day.getFullYear(),
    day.getMonth(),
    day.getDate(),
    hour,
    15,
  ).toISOString();
const row = (id: string, name: string, boughtAtUtc: string, extra = {}) => ({
  id,
  name,
  quantity: null,
  note: null,
  departmentId: 'd-dairy',
  departmentName: 'Молочка',
  departmentIcon: '🥛',
  departmentSortOrder: 1,
  urgent: false,
  status: 'Bought',
  addedBy: 'Vitaliy',
  addedAtUtc: '2026-11-01T08:00:00Z',
  boughtBy: 'Maria',
  boughtAtUtc,
  version: 3,
  ...extra,
});

const json = (body: unknown, status = 200) =>
  Promise.resolve(
    new Response(JSON.stringify(body), {
      status,
      headers: { 'Content-Type': 'application/json' },
    }),
  );

function renderHistory(items: unknown[], totalCount = items.length) {
  const sent: { method: string; url: URL; headers: Headers; body: unknown }[] =
    [];
  vi.stubGlobal('fetch', async (request: Request) => {
    const text = await request.text();
    sent.push({
      method: request.method,
      url: new URL(request.url),
      headers: request.headers,
      body: text ? JSON.parse(text) : undefined,
    });
    if (request.method === 'GET') {
      const page = Number(new URL(request.url).searchParams.get('page'));
      return page === 1
        ? json({ items, page: 1, pageSize: 50, totalCount })
        : json({
            items: [row('old', 'Соль', at(new Date(2025, 0, 3), 9))],
            page,
            pageSize: 50,
            totalCount,
          });
    }
    if (request.method === 'POST')
      return json(
        { added: [row('new', 'Молоко', '')], restored: [], updated: [] },
        201,
      );
    return new Response(null, { status: 204 });
  });
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter>
          <HistoryPage />
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  return { writes: () => sent.filter((s) => s.method !== 'GET'), sent };
}

test('groups the history by day with who bought each item', async () => {
  renderHistory([
    row('a', 'Молоко', at(today, 11), { quantity: '2 л' }),
    row('b', 'Хлеб', at(today, 9)),
    row('c', 'Сыр', at(yesterday, 18)),
  ]);

  const todayCard = await screen.findByRole('region', { name: 'Сьогодні' });
  const rows = within(todayCard).getAllByRole('listitem');
  expect(rows).toHaveLength(2);
  expect(rows[0]).toHaveTextContent('Молоко · 2 л');
  expect(rows[0]).toHaveTextContent('Маша · 11:15');
  expect(
    within(screen.getByRole('region', { name: 'Вчора' })).getByText('Сыр'),
  ).toBeInTheDocument();
});

test('returns an item to the list and deletes a row after asking', async () => {
  const { writes } = renderHistory([
    row('a', 'Молоко', at(today, 11), { quantity: '2 л' }),
  ]);
  await screen.findByRole('region', { name: 'Сьогодні' });

  await userEvent.click(
    screen.getByRole('button', { name: 'Повернути до списку: Молоко' }),
  );
  expect(
    await screen.findByText('Повернули до списку: Молоко'),
  ).toBeInTheDocument();
  const [post] = writes();
  expect(post!.url.pathname).toBe('/api/shopping/items');
  expect(post!.headers.get('Idempotency-Key')).toBeTruthy();
  expect(post!.body).toEqual({
    text: 'Молоко 2 л',
    addedBy: 'Vitaliy',
    departmentId: 'd-dairy',
    urgent: false,
  });

  await userEvent.click(
    screen.getByRole('button', { name: 'Видалити: Молоко' }),
  );
  const dialog = await screen.findByRole('dialog', {
    name: 'Видалити з історії?',
  });
  expect(writes()).toHaveLength(1);
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Видалити' }),
  );
  await waitFor(() => expect(writes()).toHaveLength(2));
  expect(writes()[1]!.method).toBe('DELETE');
  expect(writes()[1]!.headers.get('If-Match')).toBe('3');
});

test('loads more with «Показати ще»', async () => {
  const { sent } = renderHistory([row('a', 'Молоко', at(today, 11))], 51);
  await screen.findByRole('region', { name: 'Сьогодні' });

  await userEvent.click(screen.getByRole('button', { name: 'Показати ще' }));

  expect(await screen.findByText('Соль')).toBeInTheDocument();
  expect(sent.at(-1)!.url.searchParams.get('page')).toBe('2');
});

test('an empty history says so', async () => {
  renderHistory([]);

  expect(await screen.findByText('Історії поки немає')).toBeInTheDocument();
});

test('groups days under months with counts; a month continues on the next page', async () => {
  const thisMonth = new Date(today.getFullYear(), today.getMonth(), 1);
  const lastMonth = new Date(today.getFullYear(), today.getMonth() - 1, 15);
  const title = (date: Date) => {
    const name = new Intl.DateTimeFormat(locale, { month: 'long' }).format(
      date,
    );
    return `${name.charAt(0).toUpperCase()}${name.slice(1)} ${date.getFullYear()}`;
  };
  renderHistory(
    [
      row('a', 'Молоко', at(today, 11)),
      row('b', 'Хлеб', at(thisMonth, 9)),
      row('c', 'Сыр', at(lastMonth, 18)),
    ],
    51,
  );

  const current = await screen.findByRole('heading', {
    level: 2,
    name: new RegExp(`^${title(today)} · `),
  });
  expect(current).toHaveTextContent(/· [12] покупк[аи]$/);
  const previous = screen.getByRole('region', {
    name: `${title(lastMonth)} · 1+ покупок`,
  });
  expect(within(previous).getByText('Сыр')).toBeInTheDocument();
  expect(
    within(previous).getAllByRole('heading', { level: 3 }).length,
  ).toBeGreaterThan(0);

  // The next page holds an older month: the last loaded one is complete now.
  await userEvent.click(screen.getByRole('button', { name: 'Показати ще' }));
  expect(
    await screen.findByRole('region', {
      name: `${title(lastMonth)} · 1 покупка`,
    }),
  ).toBeInTheDocument();
  expect(
    screen.getByRole('region', { name: 'Січень 2025 · 1 покупка' }),
  ).toBeInTheDocument();
});
