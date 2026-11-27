import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../../app/theme/theme';
import { createQueryClient } from '../../../../platform/query-client';
import { HomePage } from '../../page/HomePage';

afterEach(() => vi.unstubAllGlobals());

const summary = {
  neededCount: 5,
  urgentCount: 1,
  inCartCount: 2,
  boughtLast7Days: 14,
  boughtLast30Days: 52,
  departments: [
    { departmentId: 'd1', name: 'Молочка', icon: '🥛', neededCount: 3 },
    { departmentId: 'd2', name: 'Хлеб', icon: '🍞', neededCount: 2 },
  ],
  nextItems: [
    {
      id: 'i1',
      name: 'Сыр',
      quantity: '300 г',
      note: null,
      departmentId: 'd1',
      departmentName: 'Молочка',
      departmentIcon: '🥛',
      departmentSortOrder: 1,
      urgent: true,
      status: 'Needed',
      addedBy: 'Maria',
      addedAtUtc: '2026-11-26T08:00:00Z',
      boughtBy: null,
      boughtAtUtc: null,
      version: 1,
    },
  ],
  topFrequent: [
    { name: 'Молоко', timesBought: 22 },
    { name: 'Хлеб', timesBought: 3 },
  ],
};

const json = (body: unknown, status = 200) =>
  Promise.resolve(
    new Response(JSON.stringify(body), {
      status,
      headers: { 'Content-Type': 'application/json' },
    }),
  );

function renderSummary(body: unknown = summary, status = 200) {
  const urls: URL[] = [];
  vi.stubGlobal('fetch', (request: Request) => {
    urls.push(new URL(request.url));
    return json(body, status);
  });
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={['/?s=shopping']}>
          <HomePage />
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  return { urls };
}

test('the switcher opens the shopping summary with its figures and lists', async () => {
  const { urls } = renderSummary();

  const needed = await screen.findByRole('region', { name: 'Треба купити' });
  expect(needed).toHaveTextContent('5');
  expect(needed).toHaveTextContent('у кошику 2');
  expect(screen.getByRole('region', { name: 'Терміново' })).toHaveTextContent(
    '1',
  );
  expect(
    screen.getByRole('region', { name: 'Куплено за 7 днів' }),
  ).toHaveTextContent('14за 30 днів 52');
  expect(urls[0]!.pathname).toBe('/api/shopping/summary');

  const departments = within(
    screen.getByRole('list', { name: 'За відділами' }),
  ).getAllByRole('listitem');
  expect(departments[0]).toHaveTextContent('🥛 Молочка3');
  const next = within(
    screen.getByRole('list', { name: 'Купити насамперед' }),
  ).getByRole('listitem');
  expect(next).toHaveTextContent('Сыр · 300 г');
  expect(next).toHaveTextContent('Маша');
  expect(next).toHaveTextContent('Терміново');
  expect(
    screen.getByRole('list', { name: 'Купуємо найчастіше' }),
  ).toHaveTextContent('⭐ Молоко22 рази');
  expect(screen.getByRole('link', { name: 'Відкрити список' })).toHaveAttribute(
    'href',
    '/shopping',
  );
});

test('an empty list says so under the figures', async () => {
  renderSummary({
    ...summary,
    neededCount: 0,
    urgentCount: 0,
    departments: [],
    nextItems: [],
    topFrequent: [],
  });

  expect(await screen.findByText('Список порожній 🎉')).toBeInTheDocument();
});

test('says why the summary did not load', async () => {
  renderSummary({ title: 'Сервер недоступний', status: 500 }, 500);

  expect(await screen.findByRole('alert')).toHaveTextContent(
    'Не вдалося завантажити зведення',
  );
});
