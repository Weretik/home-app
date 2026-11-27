import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/client/query-client';
import { todayIso } from '../entry-editing/entry-fields';
import { ExpensesSection } from '../section/ExpensesSection';
import { ForecastPage } from './ForecastPage';

afterEach(() => vi.unstubAllGlobals());

const categories = [
  { id: 'c-car', name: 'Машина', sortOrder: 1 },
  { id: 'c-gifts', name: 'Подарки', sortOrder: 2 },
];

const plan = (overrides: Record<string, unknown> = {}) => ({
  id: crypto.randomUUID(),
  version: 3,
  year: 2026,
  month: 10,
  name: 'Страховка',
  categoryId: 'c-car',
  categoryName: 'Машина',
  payer: 'Vitaliy',
  amount: 4200,
  ...overrides,
});

/** The list as GET /api/expenses/planned answers it. */
const plans = (items: { amount: number }[]) => ({
  items,
  totalAmount: items.reduce((sum, item) => sum + item.amount, 0),
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

/** Stubs the API: categories always answer; `planned` answers everything else. */
function renderForecast(
  planned: (sent: Sent) => Promise<Response>,
  path = '/expenses/forecast?year=2026&month=10',
) {
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
    return entry.url.pathname === '/api/expenses/categories'
      ? json(categories)
      : planned(entry);
  });
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={[path]}>
          <Routes>
            <Route path="/expenses" element={<ExpensesSection />}>
              <Route path="forecast" element={<ForecastPage />} />
            </Route>
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  const reads = () =>
    sent.filter(
      (s) => s.method === 'GET' && s.url.pathname === '/api/expenses/planned',
    );
  const writes = () => sent.filter((s) => s.method !== 'GET');
  return { reads, writes };
}

const plain = (element: HTMLElement) =>
  element.textContent?.replace(/\s/g, ' ');

const dialogClosed = () =>
  waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument());

test('lists the month plans with their categories and the total', async () => {
  const { reads } = renderForecast(() =>
    json(
      plans([
        plan(),
        plan({
          name: 'Подарок маме',
          categoryId: 'c-gifts',
          categoryName: 'Подарки',
          payer: 'Shared',
          amount: 1500.5,
        }),
      ]),
    ),
  );

  const list = await screen.findByRole('list', {
    name: 'Заплановані витрати',
  });
  expect(screen.getByRole('heading', { name: 'Бюджет' })).toBeInTheDocument();
  expect(screen.getByRole('tab', { name: 'Прогноз' })).toHaveAttribute(
    'aria-current',
    'page',
  );
  const rows = within(list).getAllByRole('listitem');
  expect(rows).toHaveLength(2);
  expect(plain(rows[0]!)).toContain('СтраховкаМашина · Віталій');
  expect(plain(rows[1]!)).toContain('Подарки · Спільне');
  await waitFor(() =>
    expect(plain(screen.getByRole('status'))).toContain(
      'Заплановано 5 700,50 ₴',
    ),
  );
  const query = reads()[0]!.url.searchParams;
  expect(query.get('year')).toBe('2026');
  expect(query.get('month')).toBe('10');
});

test('says when nothing is planned', async () => {
  renderForecast(() => json(plans([])));

  expect(
    await screen.findByText('На цей місяць нічого не заплановано'),
  ).toBeInTheDocument();
});

test('adds a plan to the open month with an Idempotency-Key', async () => {
  const { writes, reads } = renderForecast((sent) =>
    sent.method === 'POST'
      ? json({ id: crypto.randomUUID() }, 201)
      : json(plans([])),
  );
  await screen.findByText('На цей місяць нічого не заплановано');

  await userEvent.click(screen.getByRole('button', { name: 'Додати план' }));
  const dialog = await screen.findByRole('dialog', { name: 'Новий план' });
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  expect(within(dialog).getByText('Введіть назву')).toBeInTheDocument();
  expect(writes()).toHaveLength(0);

  await userEvent.type(within(dialog).getByLabelText(/Назва/), 'Подарок');
  await userEvent.click(
    within(dialog).getByRole('combobox', { name: /Категорія/ }),
  );
  await userEvent.click(await screen.findByRole('option', { name: 'Подарки' }));
  await userEvent.type(within(dialog).getByLabelText(/Сума/), '1500');
  const loads = reads().length;
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await dialogClosed();

  const [post] = writes();
  expect(post!.url.pathname).toBe('/api/expenses/planned');
  expect(post!.headers.get('Idempotency-Key')).toBeTruthy();
  expect(post!.body).toEqual({
    year: 2026,
    month: 10,
    name: 'Подарок',
    categoryId: 'c-gifts',
    payer: 'Shared',
    amount: 1500,
  });
  await waitFor(() => expect(reads().length).toBeGreaterThan(loads));
});

test('records a plan as an expense with its version in If-Match', async () => {
  const item = plan();
  const { writes } = renderForecast((sent) =>
    sent.method === 'POST'
      ? json({ id: crypto.randomUUID() }, 201)
      : json(plans([item])),
  );
  await screen.findByRole('list', { name: 'Заплановані витрати' });

  await userEvent.click(screen.getByRole('button', { name: /Дії: Страховка/ }));
  await userEvent.click(
    screen.getByRole('menuitem', { name: 'Записати як витрату' }),
  );
  const dialog = await screen.findByRole('dialog', {
    name: 'Записати як витрату',
  });
  expect(plain(dialog)).toContain('Страховка · Машина · Віталій · 4 200,00 ₴');
  expect(within(dialog).getByLabelText(/Дата/)).toHaveValue(todayIso());
  await userEvent.click(
    within(dialog).getByRole('combobox', { name: 'Оплата' }),
  );
  await userEvent.click(await screen.findByRole('option', { name: 'Готівка' }));
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Записати' }),
  );
  await dialogClosed();

  const [record] = writes();
  expect(record!.method).toBe('POST');
  expect(record!.url.pathname).toBe(`/api/expenses/planned/${item.id}/record`);
  expect(record!.headers.get('If-Match')).toBe('3');
  expect(record!.body).toEqual({
    date: todayIso(),
    paymentMethod: 'Cash',
    comment: null,
  });
});

test('deletes a plan only after the confirmation', async () => {
  const item = plan();
  const { writes } = renderForecast((sent) =>
    sent.method === 'DELETE'
      ? Promise.resolve(new Response(null, { status: 204 }))
      : json(plans([item])),
  );
  await screen.findByRole('list', { name: 'Заплановані витрати' });

  await userEvent.click(screen.getByRole('button', { name: /Дії: Страховка/ }));
  await userEvent.click(screen.getByRole('menuitem', { name: 'Видалити' }));
  const dialog = await screen.findByRole('dialog', { name: 'Видалити план?' });
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Видалити' }),
  );
  await dialogClosed();

  expect(writes()).toHaveLength(1);
  expect(writes()[0]!.url.pathname).toBe(`/api/expenses/planned/${item.id}`);
});
