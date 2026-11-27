import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../../app/theme/theme';
import { createQueryClient } from '../../../../platform/query-client';
import { GiftsSummary } from './GiftsSummary';

afterEach(() => vi.unstubAllGlobals());

const counts = (wanted: number, extra: Record<string, number> = {}) => ({
  wantedCount: wanted,
  wantedPriceSum: 0,
  wantedWithoutPrice: 0,
  highCount: 0,
  giftedCount: 0,
  giftedPriceSum: 0,
  ...extra,
});

const summary = {
  recipients: [
    {
      recipient: 'Maria',
      counts: counts(12, {
        wantedPriceSum: 45000,
        wantedWithoutPrice: 3,
        highCount: 4,
        giftedCount: 7,
        giftedPriceSum: 20000,
      }),
      topWanted: [
        { id: 't1', name: 'Серьги', price: null, link: 'https://shop.ua/1' },
        { id: 't2', name: 'Духи', price: 3200, link: null },
      ],
    },
    { recipient: 'Vitaliy', counts: counts(5), topWanted: [] },
    { recipient: 'Together', counts: counts(2), topWanted: [] },
  ],
  total: counts(19, {
    wantedPriceSum: 60000,
    wantedWithoutPrice: 5,
    highCount: 6,
    giftedCount: 9,
    giftedPriceSum: 25000,
  }),
  upcoming: [
    {
      id: 'u1',
      recipient: 'Vitaliy',
      name: 'Наушники',
      occasion: 'День рождения',
      occasionDate: '2027-03-12',
    },
  ],
  details: [
    {
      id: 'd1',
      version: 1,
      recipient: 'Maria',
      label: 'Кольцо',
      value: '18,5–19,0',
      sortOrder: 1,
    },
    {
      id: 'd2',
      version: 1,
      recipient: 'Vitaliy',
      label: 'Обувь',
      value: '43',
      sortOrder: 1,
    },
  ],
};

const empty = {
  recipients: ['Maria', 'Vitaliy', 'Together'].map((recipient) => ({
    recipient,
    counts: counts(0),
    topWanted: [],
  })),
  total: counts(0),
  upcoming: [],
  details: [],
};

function renderSummary(answer: () => Promise<Response>) {
  const urls: URL[] = [];
  vi.stubGlobal('fetch', (request: Request) => {
    urls.push(new URL(request.url));
    return answer();
  });
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter>
          <GiftsSummary />
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  return urls;
}

const json = (body: unknown, status = 200) =>
  Promise.resolve(
    new Response(JSON.stringify(body), {
      status,
      headers: { 'Content-Type': 'application/json' },
    }),
  );
const plain = (element: HTMLElement) =>
  element.textContent?.replace(/\s/g, ' ');

test('shows the figures per person and in total', async () => {
  const urls = renderSummary(() => json(summary));
  const masha = await screen.findByRole('region', { name: 'Маша' });
  expect(plain(masha)).toContain('12 бажань');
  expect(plain(masha)).toContain('≈ 45 000 ₴ · 3 без ціни');
  expect(plain(masha)).toContain('🔥 4 · подаровано 7 на 20 000 ₴');
  expect(plain(screen.getByRole('region', { name: 'Усього' }))).toContain(
    '19 бажань',
  );
  expect(urls[0]!.pathname).toBe('/api/gifts/summary');
  expect(
    screen.getByRole('link', { name: 'Відкрити подарунки' }),
  ).toHaveAttribute('href', '/gifts');
});

test('lists the 🔥 wishes, the nearest occasions and the нюанси read-only', async () => {
  renderSummary(() => json(summary));
  const top = await screen.findByRole('region', { name: '🔥 Дуже хочу' });
  const masha = within(top).getByRole('group', { name: 'Маша' });
  expect(plain(masha)).toContain('🔥 4 бажання · ≈ 3 200 ₴');
  expect(plain(masha)).toContain('І ще 2 у розділі «Подарунки»');
  expect(within(top).getByRole('group', { name: 'Віталій' })).toHaveTextContent(
    'Поки немає бажань з 🔥',
  );
  expect(within(top).getByRole('group', { name: 'Разом' })).toBeInTheDocument();
  expect(within(top).getByText('Серьги')).toBeInTheDocument();
  expect(within(top).getByText('ціну не вказано')).toBeInTheDocument();
  expect(
    within(top).getByRole('link', { name: 'Відкрити посилання: Серьги' }),
  ).toHaveAttribute('href', 'https://shop.ua/1');
  const occasions = screen.getByRole('region', { name: 'Найближчі нагоди' });
  expect(plain(occasions)).toContain('12 березня 2027');
  expect(plain(occasions)).toContain('Наушники');
  expect(plain(occasions)).toContain('Віталій · День рождения');
  const details = screen.getByRole('region', { name: 'Нюанси' });
  expect(within(details).getByText('18,5–19,0')).toBeInTheDocument();
  expect(within(details).queryByRole('button', { name: /Дії/ })).toBeNull();
  expect(
    within(details).getByRole('link', { name: 'Змінити' }),
  ).toHaveAttribute('href', '/gifts/settings');
});

test('says when there are no wishes and no нюанси yet', async () => {
  renderSummary(() => json(empty));
  expect(await screen.findByText('Бажань поки немає')).toBeInTheDocument();
  expect(screen.getByText(/Нюансів поки немає/)).toBeInTheDocument();
  expect(screen.queryByRole('region', { name: '🔥 Дуже хочу' })).toBeNull();
});

test('says why the summary did not load and retries', async () => {
  let fail = true;
  renderSummary(() =>
    fail ? json({ title: 'Сервер недоступний' }, 503) : json(summary),
  );
  expect(
    await screen.findByText(
      /Не вдалося завантажити зведення: Сервер недоступний/,
    ),
  ).toBeInTheDocument();
  fail = false;
  await userEvent.click(screen.getByRole('button', { name: 'Повторити' }));
  expect(
    await screen.findByRole('region', { name: 'Маша' }),
  ).toBeInTheDocument();
});
