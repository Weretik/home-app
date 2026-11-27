import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, beforeEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/client/query-client';
import { formatAmount } from '../data/formatting';
import { ExpensesSection } from '../section/ExpensesSection';
import { SettingsPage } from './SettingsPage';

// Only Date is faked, so «now» is 10 December 2026 in Kyiv while timers and user events run as usual.
beforeEach(() => {
  vi.useFakeTimers({ toFake: ['Date'] });
  vi.setSystemTime(new Date('2026-12-10T10:00:00Z'));
});
afterEach(() => {
  vi.useRealTimers();
  vi.unstubAllGlobals();
});

const categories = [
  { id: 'c-food', name: 'Продукты', sortOrder: 1 },
  { id: 'c-home', name: 'Быт', sortOrder: 2 },
  { id: 'c-cafe', name: 'Кафе', sortOrder: 3 },
];

const budget = {
  version: 3,
  monthlyLimit: 50_000,
  monthlyLimits: [
    { from: '2000-01-01', amount: 45_000 },
    { from: '2026-12-01', amount: 50_000 },
  ],
  savingsGoal: 100_000,
  limits: [
    {
      id: 'l-food',
      version: 2,
      name: 'Еда и быт',
      period: 'Month',
      amount: 15_000,
      amounts: [
        { from: '2000-01-01', amount: 7_000 },
        { from: '2026-12-01', amount: 15_000 },
      ],
      categoryIds: ['c-food', 'c-home'],
    },
    {
      id: 'l-cafe',
      version: 1,
      name: 'Кафе',
      period: 'Quarter',
      amount: 3_000,
      amounts: [{ from: '2026-10-01', amount: 3_000 }],
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

function renderSettings(failWrite?: () => Promise<Response>) {
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
    if (failWrite && entry.method !== 'GET') return failWrite();
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

/** An amount as text matchers see it: Testing Library collapses the formatter's no-break spaces. */
const money = (value: number) => formatAmount(value).replace(/\s+/g, ' ');

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
  expect(put!.body).toEqual({
    monthlyLimit: 0,
    savingsGoal: 120000,
    monthlyLimitFrom: '2026-12-01',
  });
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
    amountFrom: '2026-12-01',
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

test('SC-WLA-001: a changed amount applies from the current month by default', async () => {
  const { writes } = renderSettings();
  await screen.findByRole('list', { name: 'Ліміти' });

  await userEvent.click(screen.getByRole('button', { name: /Дії: Еда и быт/ }));
  await userEvent.click(screen.getByRole('menuitem', { name: 'Змінити' }));
  const dialog = await screen.findByRole('dialog', { name: 'Змінити ліміт' });
  const month = within(dialog).getByLabelText(/Діє з/);
  expect(month).toHaveValue('2026-12');
  expect(dialog).toHaveTextContent('Попередні місяці зберігають свій ліміт');
  const amount = within(dialog).getByLabelText(/Сума ліміту/);
  await userEvent.clear(amount);
  await userEvent.type(amount, '5000');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await dialogClosed();

  const [put] = writes();
  expect(put!.url.pathname).toBe('/api/expenses/budget/limits/l-food');
  expect(put!.headers.get('If-Match')).toBe('2');
  expect(put!.body).toMatchObject({ amount: 5000, amountFrom: '2026-12-01' });
});

test('a quarterly limit says the amount starts with the quarter; a cleared month blocks saving', async () => {
  const { writes } = renderSettings();
  await screen.findByRole('list', { name: 'Ліміти' });

  await userEvent.click(screen.getByRole('button', { name: /Дії: Кафе/ }));
  await userEvent.click(screen.getByRole('menuitem', { name: 'Змінити' }));
  const dialog = await screen.findByRole('dialog', { name: 'Змінити ліміт' });
  expect(dialog).toHaveTextContent(/з початку кварталу/);
  fireEvent.change(within(dialog).getByLabelText(/Діє з/), {
    target: { value: '' },
  });
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  expect(within(dialog).getByText('Виберіть місяць')).toBeInTheDocument();
  expect(writes()).toHaveLength(0);
});

test('the API rejecting the month shows under the month field', async () => {
  renderSettings(() =>
    json(
      {
        title: 'Validation failed',
        status: 400,
        errors: { AmountFrom: ['Місяць поза межами'] },
      },
      400,
    ),
  );
  await screen.findByRole('list', { name: 'Ліміти' });

  await userEvent.click(screen.getByRole('button', { name: /Дії: Кафе/ }));
  await userEvent.click(screen.getByRole('menuitem', { name: 'Змінити' }));
  const dialog = await screen.findByRole('dialog', { name: 'Змінити ліміт' });
  const month = within(dialog).getByLabelText(/Діє з/);
  fireEvent.change(month, { target: { value: '2027-02' } });
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );

  expect(await within(dialog).findByText('Місяць поза межами')).toBeVisible();
  expect(month).toHaveAttribute('aria-invalid', 'true');
});

test('SC-WLA-002: the monthly limit is saved from the chosen month', async () => {
  const { writes } = renderSettings();
  const month = await screen.findByLabelText(/Ліміт діє з/);
  expect(month).toHaveAttribute('placeholder', 'РРРР-ММ');
  expect(month).toHaveValue('2026-12');

  fireEvent.change(month, { target: { value: '2027-01' } });
  await userEvent.click(screen.getByRole('button', { name: 'Зберегти' }));

  expect(await screen.findByText('Збережено')).toBeInTheDocument();
  expect(writes()[0]!.body).toEqual({
    monthlyLimit: 50000,
    savingsGoal: 100000,
    monthlyLimitFrom: '2027-01-01',
  });
});

test('SC-WLA-003: a changed limit shows since when and the amount before; one amount shows none', async () => {
  renderSettings();
  const list = await screen.findByRole('list', { name: 'Ліміти' });
  const [food, cafe] = within(list).getAllByRole('listitem');

  expect(food).toHaveTextContent(money(15_000));
  expect(food).toHaveTextContent(`з грудня 2026 · раніше ${money(7_000)}`);
  expect(cafe).not.toHaveTextContent('раніше');
  expect(
    screen.getByText(
      `з грудня 2026 · раніше ${money(45_000)}; порожньо — без ліміту`,
    ),
  ).toBeInTheDocument();
});

test('SC-WLA-004: a new limit has no «Діє з» month and sends none', async () => {
  const { writes } = renderSettings();
  await screen.findByRole('list', { name: 'Ліміти' });

  await userEvent.click(screen.getByRole('button', { name: 'Додати ліміт' }));
  const dialog = await screen.findByRole('dialog', { name: 'Новий ліміт' });
  expect(within(dialog).queryByLabelText(/Діє з/)).not.toBeInTheDocument();

  await userEvent.type(within(dialog).getByLabelText(/Назва/), 'Їжа');
  await userEvent.type(within(dialog).getByLabelText(/Сума ліміту/), '7000');
  await userEvent.click(
    within(dialog).getByRole('combobox', { name: /Категорії/ }),
  );
  await userEvent.click(await screen.findByRole('option', { name: 'Кафе' }));
  await userEvent.keyboard('{Escape}');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await dialogClosed();

  expect(writes()[0]!.method).toBe('POST');
  expect(writes()[0]!.body).not.toHaveProperty('amountFrom');
});

test('SC-WLA-005: a rename sends no «Діє з», so a change saved ahead stays', async () => {
  const { writes } = renderSettings();
  await screen.findByRole('list', { name: 'Ліміти' });

  await userEvent.click(screen.getByRole('button', { name: /Дії: Еда и быт/ }));
  await userEvent.click(screen.getByRole('menuitem', { name: 'Змінити' }));
  const dialog = await screen.findByRole('dialog', { name: 'Змінити ліміт' });
  const name = within(dialog).getByLabelText(/Назва/);
  await userEvent.clear(name);
  await userEvent.type(name, 'Їжа');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await dialogClosed();

  const [put] = writes();
  expect(put!.body).toEqual({
    name: 'Їжа',
    period: 'Month',
    amount: 15000,
    categoryIds: ['c-food', 'c-home'],
  });
});

test('SC-WLA-005: a goal-only save sends no monthlyLimitFrom', async () => {
  const { writes } = renderSettings();
  const goal = await screen.findByLabelText('План скарбнички');

  await userEvent.clear(goal);
  await userEvent.type(goal, '120 000');
  await userEvent.click(screen.getByRole('button', { name: 'Зберегти' }));

  expect(await screen.findByText('Збережено')).toBeInTheDocument();
  expect(writes()[0]!.body).toEqual({
    monthlyLimit: 50000,
    savingsGoal: 120000,
  });
});

test('SC-WLA-006: a change saved ahead shows its month and amount', async () => {
  budget.limits[1]!.amounts = [
    { from: '2026-10-01', amount: 3_000 },
    { from: '2027-04-01', amount: 6_000 },
  ];
  budget.monthlyLimits.push({ from: '2027-04-01', amount: 55_000 });
  try {
    renderSettings();
    const list = await screen.findByRole('list', { name: 'Ліміти' });
    const cafe = within(list).getAllByRole('listitem')[1]!;

    expect(cafe).toHaveTextContent(money(3_000));
    expect(cafe).toHaveTextContent(`з квітня 2027 — ${money(6_000)}`);
    expect(cafe).not.toHaveTextContent('раніше');
    expect(
      screen.getByText(
        `з грудня 2026 · раніше ${money(45_000)}; з квітня 2027 — ${money(55_000)}; порожньо — без ліміту`,
      ),
    ).toBeInTheDocument();
  } finally {
    budget.limits[1]!.amounts = [{ from: '2026-10-01', amount: 3_000 }];
    budget.monthlyLimits.pop();
  }
});
