import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/query-client';
import { ExpensesSection } from '../section/ExpensesSection';
import { SettingsPage } from './SettingsPage';

afterEach(() => vi.unstubAllGlobals());

const categories = [
  { id: 'c-food', name: 'Продукты', sortOrder: 1 },
  { id: 'c-home', name: 'Быт', sortOrder: 2 },
  { id: 'c-cafe', name: 'Кафе', sortOrder: 3 },
];

const budget = {
  version: 3,
  monthlyLimit: 50_000,
  savingsGoal: 100_000,
  limits: [
    {
      id: 'l-food',
      version: 2,
      name: 'Еда и быт',
      period: 'Month',
      amount: 15_000,
      categoryIds: ['c-food', 'c-home'],
    },
    {
      id: 'l-cafe',
      version: 1,
      name: 'Кафе',
      period: 'Quarter',
      amount: 3_000,
      categoryIds: ['c-cafe'],
    },
  ],
};

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

function renderSettings() {
  const sent: Sent[] = [];
  vi.stubGlobal('fetch', async (request: Request) => {
    const text = await request.text();
    const entry: Sent = {
      method: request.method,
      url: new URL(request.url),
      headers: request.headers,
      body: text ? JSON.parse(text) : undefined,
    };
    sent.push(entry);
    if (entry.method === 'POST') return json({ id: crypto.randomUUID() }, 201);
    if (entry.method !== 'GET') return new Response(null, { status: 204 });
    return entry.url.pathname === '/api/expenses/categories'
      ? json(categories)
      : json(budget);
  });
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter
          initialEntries={['/expenses/settings?year=2026&month=10']}
        >
          <Routes>
            <Route path="/expenses" element={<ExpensesSection />}>
              <Route path="settings" element={<SettingsPage />} />
            </Route>
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  const writes = () => sent.filter((s) => s.method !== 'GET');
  return { writes };
}

const dialogClosed = () =>
  waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument());

test('shows the monthly limit, the savings goal and the limits', async () => {
  renderSettings();

  expect(
    await screen.findByLabelText('Загальний ліміт витрат на місяць'),
  ).toHaveValue('50000');
  expect(screen.getByLabelText('План скарбнички')).toHaveValue('100000');
  const list = screen.getByRole('list', { name: 'Ліміти' });
  const rows = within(list).getAllByRole('listitem');
  expect(rows).toHaveLength(2);
  expect(rows[0]).toHaveTextContent('Еда и быт');
  expect(rows[0]).toHaveTextContent('на місяць');
  expect(await within(rows[0]!).findByText('Продукты')).toBeInTheDocument();
  expect(within(rows[0]!).getByText('Быт')).toBeInTheDocument();
  expect(rows[1]).toHaveTextContent('на квартал');

  expect(
    screen.queryByRole('group', { name: 'Місяць' }),
  ).not.toBeInTheDocument();
  const gear = screen.getByRole('link', { name: 'Налаштування' });
  expect(gear).toHaveAttribute('aria-current', 'page');
  expect(gear).toHaveAttribute('href', '/expenses/settings?year=2026&month=10');
});

test('saves the limit and the goal with the version in If-Match', async () => {
  const { writes } = renderSettings();
  const goal = await screen.findByLabelText('План скарбнички');

  await userEvent.clear(goal);
  await userEvent.type(goal, '120 000');
  await userEvent.clear(
    screen.getByLabelText('Загальний ліміт витрат на місяць'),
  );
  await userEvent.click(screen.getByRole('button', { name: 'Зберегти' }));

  expect(await screen.findByText('Збережено')).toBeInTheDocument();
  const [put] = writes();
  expect(put!.method).toBe('PUT');
  expect(put!.url.pathname).toBe('/api/expenses/budget/settings');
  expect(put!.headers.get('If-Match')).toBe('3');
  expect(put!.body).toEqual({ monthlyLimit: 0, savingsGoal: 120000 });
});

test('checks the amounts before saving the settings', async () => {
  const { writes } = renderSettings();
  const goal = await screen.findByLabelText('План скарбнички');

  await userEvent.clear(goal);
  await userEvent.type(goal, 'много');
  await userEvent.click(screen.getByRole('button', { name: 'Зберегти' }));

  expect(
    screen.getByText('Сума — число, не більше двох знаків після коми'),
  ).toBeInTheDocument();
  expect(writes()).toHaveLength(0);
});

test('adds a limit with an Idempotency-Key and its categories', async () => {
  const { writes } = renderSettings();
  await screen.findByRole('list', { name: 'Ліміти' });

  await userEvent.click(screen.getByRole('button', { name: 'Додати ліміт' }));
  const dialog = await screen.findByRole('dialog', { name: 'Новий ліміт' });
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  expect(within(dialog).getByText('Введіть назву')).toBeInTheDocument();
  expect(
    within(dialog).getByText('Виберіть хоча б одну категорію'),
  ).toBeInTheDocument();
  expect(writes()).toHaveLength(0);

  await userEvent.type(within(dialog).getByLabelText(/Назва/), 'Зоотовары');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'на квартал' }),
  );
  await userEvent.type(within(dialog).getByLabelText(/Сума ліміту/), '2500');
  await userEvent.click(
    within(dialog).getByRole('combobox', { name: /Категорії/ }),
  );
  await userEvent.click(await screen.findByRole('option', { name: 'Кафе' }));
  await userEvent.click(screen.getByRole('option', { name: 'Продукты' }));
  await userEvent.keyboard('{Escape}');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await dialogClosed();

  const [post] = writes();
  expect(post!.method).toBe('POST');
  expect(post!.url.pathname).toBe('/api/expenses/budget/limits');
  expect(post!.headers.get('Idempotency-Key')).toBeTruthy();
  expect(post!.body).toEqual({
    name: 'Зоотовары',
    period: 'Quarter',
    amount: 2500,
    categoryIds: ['c-cafe', 'c-food'],
  });
});

test('changes a limit with its version in If-Match', async () => {
  const { writes } = renderSettings();
  await screen.findByRole('list', { name: 'Ліміти' });

  await userEvent.click(screen.getByRole('button', { name: /Дії: Кафе/ }));
  await userEvent.click(screen.getByRole('menuitem', { name: 'Змінити' }));
  const dialog = await screen.findByRole('dialog', { name: 'Змінити ліміт' });
  const amount = within(dialog).getByLabelText(/Сума ліміту/);
  expect(amount).toHaveValue('3000');
  await userEvent.clear(amount);
  await userEvent.type(amount, '4000');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await dialogClosed();

  const [put] = writes();
  expect(put!.url.pathname).toBe('/api/expenses/budget/limits/l-cafe');
  expect(put!.headers.get('If-Match')).toBe('1');
  expect(put!.body).toEqual({
    name: 'Кафе',
    period: 'Quarter',
    amount: 4000,
    categoryIds: ['c-cafe'],
  });
});

test('deletes a limit only after the confirmation', async () => {
  const { writes } = renderSettings();
  await screen.findByRole('list', { name: 'Ліміти' });

  await userEvent.click(screen.getByRole('button', { name: /Дії: Еда и быт/ }));
  await userEvent.click(screen.getByRole('menuitem', { name: 'Видалити' }));
  const dialog = await screen.findByRole('dialog', { name: 'Видалити ліміт?' });
  expect(dialog).toHaveTextContent('Еда и быт');
  expect(writes()).toHaveLength(0);
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Видалити' }),
  );
  await dialogClosed();

  expect(writes()).toHaveLength(1);
  expect(writes()[0]!.method).toBe('DELETE');
  expect(writes()[0]!.url.pathname).toBe('/api/expenses/budget/limits/l-food');
});
