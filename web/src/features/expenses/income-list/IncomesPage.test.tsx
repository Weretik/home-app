import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/query-client';
import { ExpensesSection } from '../section/ExpensesSection';
import { IncomesPage } from './IncomesPage';

afterEach(() => vi.unstubAllGlobals());

const income = (overrides: Record<string, unknown> = {}) => ({
  id: crypto.randomUUID(),
  version: 1,
  date: '2026-10-05',
  person: 'Vitaliy',
  amount: 50000,
  comment: 'Зарплата',
  ...overrides,
});

const page = (items: unknown[], totalAmount: number) => ({
  items,
  page: 1,
  pageSize: 50,
  totalCount: items.length,
  totalAmount,
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

function renderPage(answer: (url: URL, sent: Sent) => Promise<Response>) {
  const sent: Sent[] = [];
  vi.stubGlobal('fetch', async (input: Request | string) => {
    const request = typeof input === 'string' ? new Request(input) : input;
    const text = await request.text();
    const entry: Sent = {
      method: request.method,
      url: new URL(request.url),
      headers: request.headers,
      body: text ? JSON.parse(text) : undefined,
    };
    sent.push(entry);
    return answer(entry.url, entry);
  });
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={['/expenses/incomes?year=2026&month=10']}>
          <Routes>
            <Route path="/expenses" element={<ExpensesSection />}>
              <Route path="incomes" element={<IncomesPage />} />
            </Route>
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  const reads = () => sent.filter((s) => s.method === 'GET');
  return {
    lastUrl: () => reads().at(-1)!.url,
    listLoads: () => reads().length,
    writes: () => sent.filter((s) => s.method !== 'GET'),
  };
}

const noContent = () => Promise.resolve(new Response(null, { status: 204 }));
const dialogClosed = () =>
  waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument());

const normalized = (text: string) => (_: string, element: Element | null) =>
  element?.textContent?.replace(/\s/g, ' ') === text &&
  !Array.from(element.children).some(
    (child) => child.textContent?.replace(/\s/g, ' ') === text,
  );

test('shows each income and no month total card', async () => {
  const { lastUrl } = renderPage(() =>
    json(
      page(
        [income(), income({ person: 'Maria', amount: 30000, comment: null })],
        80000,
      ),
    ),
  );

  const rows = within(
    await screen.findByRole('list', { name: 'Доходы за месяц' }),
  ).getAllByRole('listitem');
  expect(screen.queryByText(normalized('80 000,00 ₴'))).not.toBeInTheDocument();
  expect(rows[0]).toHaveTextContent('Виталий');
  expect(rows[0]).toHaveTextContent('Зарплата');
  expect(rows[1]).toHaveTextContent('Маша');
  expect(screen.getByRole('tab', { name: 'Доходы' })).toHaveAttribute(
    'aria-current',
    'page',
  );
  expect(lastUrl().pathname).toBe('/api/expenses/incomes');
  expect(lastUrl().searchParams.get('month')).toBe('10');
});

test('filters by person', async () => {
  const { lastUrl } = renderPage(() => json(page([income()], 50000)));
  await screen.findByRole('list', { name: 'Доходы за месяц' });

  await userEvent.click(screen.getByRole('combobox', { name: 'Кто' }));
  await userEvent.click(screen.getByRole('option', { name: 'Маша' }));

  expect(lastUrl().searchParams.get('person')).toBe('Maria');
});

test('says when the month has no incomes', async () => {
  renderPage(() => json(page([], 0)));

  expect(
    await screen.findByText('За этот месяц записей нет'),
  ).toBeInTheDocument();
});

test('shows an error when the API fails', async () => {
  renderPage(() => json({ title: 'Ошибка' }, 500));

  expect(await screen.findByRole('alert')).toHaveTextContent(
    'Не удалось загрузить',
  );
});

test('adds an income with an Idempotency-Key and reloads the list', async () => {
  const { writes, listLoads } = renderPage((_, sent) =>
    sent.method === 'POST'
      ? json({ id: crypto.randomUUID() }, 201)
      : json(page([income()], 50000)),
  );
  await screen.findByRole('list', { name: 'Доходы за месяц' });

  await userEvent.click(screen.getByRole('button', { name: 'Добавить доход' }));
  const dialog = await screen.findByRole('dialog', { name: 'Новый доход' });
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );
  expect(within(dialog).getByText('Введите сумму')).toBeInTheDocument();
  expect(writes()).toHaveLength(0);

  await userEvent.click(within(dialog).getByRole('combobox', { name: 'Кто' }));
  await userEvent.click(await screen.findByRole('option', { name: 'Маша' }));
  await userEvent.type(within(dialog).getByLabelText(/Сумма/), '30000');
  await userEvent.type(within(dialog).getByLabelText('Комментарий'), 'Аванс');
  const loads = listLoads();
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );
  await dialogClosed();

  const [post] = writes();
  expect(post!.method).toBe('POST');
  expect(post!.url.pathname).toBe('/api/expenses/incomes');
  expect(post!.headers.get('Idempotency-Key')).toBeTruthy();
  expect(post!.body).toEqual({
    date: '2026-10-01',
    person: 'Maria',
    amount: 30000,
    comment: 'Аванс',
  });
  await waitFor(() => expect(listLoads()).toBeGreaterThan(loads));
});

test('changes an income with its version in If-Match', async () => {
  const item = income({ version: 3 });
  const { writes } = renderPage((_, sent) =>
    sent.method === 'PUT' ? noContent() : json(page([item], 50000)),
  );
  await screen.findByRole('list', { name: 'Доходы за месяц' });

  await userEvent.click(
    screen.getByRole('button', { name: /Действия: Виталий/ }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Изменить' }));
  const dialog = await screen.findByRole('dialog', { name: 'Изменить доход' });
  const amount = within(dialog).getByLabelText(/Сумма/);
  expect(amount).toHaveValue('50000');
  await userEvent.clear(amount);
  await userEvent.type(amount, '52000');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );
  await dialogClosed();

  const [put] = writes();
  expect(put!.method).toBe('PUT');
  expect(put!.url.pathname).toBe(`/api/expenses/incomes/${item.id}`);
  expect(put!.headers.get('If-Match')).toBe('3');
  expect(put!.headers.has('Idempotency-Key')).toBe(false);
  expect(put!.body).toEqual({
    date: '2026-10-05',
    person: 'Vitaliy',
    amount: 52000,
    comment: 'Зарплата',
  });
});

test('deletes an income only after the confirmation', async () => {
  const item = income();
  const { writes } = renderPage((_, sent) =>
    sent.method === 'DELETE' ? noContent() : json(page([item], 50000)),
  );
  await screen.findByRole('list', { name: 'Доходы за месяц' });

  await userEvent.click(
    screen.getByRole('button', { name: /Действия: Виталий/ }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Удалить' }));
  const dialog = await screen.findByRole('dialog', { name: 'Удалить доход?' });
  expect(writes()).toHaveLength(0);
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Удалить' }),
  );
  await dialogClosed();

  expect(writes()).toHaveLength(1);
  expect(writes()[0]!.method).toBe('DELETE');
  expect(writes()[0]!.url.pathname).toBe(`/api/expenses/incomes/${item.id}`);
});
