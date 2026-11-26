import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, within } from '@testing-library/react';
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

function renderPage(answer: (url: URL) => Promise<Response>) {
  const fetchMock = vi.fn((input: Request | string) =>
    answer(new URL(typeof input === 'string' ? input : input.url)),
  );
  vi.stubGlobal('fetch', fetchMock);
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
  return {
    lastUrl: () => {
      const input = fetchMock.mock.calls.at(-1)![0];
      return new URL(typeof input === 'string' ? input : input.url);
    },
  };
}

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
