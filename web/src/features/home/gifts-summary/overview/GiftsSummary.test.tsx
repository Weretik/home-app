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
  expect(plain(masha)).toContain('12 желаний');
  expect(plain(masha)).toContain('≈ 45 000 ₴ · 3 без цены');
  expect(plain(masha)).toContain('🔥 4 · подарено 7 на 20 000 ₴');
  expect(plain(screen.getByRole('region', { name: 'Всего' }))).toContain(
    '19 желаний',
  );
  expect(urls[0]!.pathname).toBe('/api/gifts/summary');
  expect(screen.getByRole('link', { name: 'Открыть подарки' })).toHaveAttribute(
    'href',
    '/gifts',
  );
});

test('lists the 🔥 wishes, the nearest occasions and the нюансы read-only', async () => {
  renderSummary(() => json(summary));
  const top = await screen.findByRole('region', { name: '🔥 Очень хочу' });
  const masha = within(top).getByRole('group', { name: 'Маша' });
  expect(plain(masha)).toContain('🔥 4 желания · ≈ 3 200 ₴');
  expect(plain(masha)).toContain('И ещё 2 в разделе «Подарки»');
  expect(within(top).getByRole('group', { name: 'Виталий' })).toHaveTextContent(
    'Пока нет желаний с 🔥',
  );
  expect(
    within(top).getByRole('group', { name: 'Вместе' }),
  ).toBeInTheDocument();
  expect(within(top).getByText('Серьги')).toBeInTheDocument();
  expect(within(top).getByText('цена не указана')).toBeInTheDocument();
  expect(
    within(top).getByRole('link', { name: 'Открыть ссылку: Серьги' }),
  ).toHaveAttribute('href', 'https://shop.ua/1');
  const occasions = screen.getByRole('region', { name: 'Ближайшие поводы' });
  expect(plain(occasions)).toContain('12 марта 2027');
  expect(plain(occasions)).toContain('Наушники');
  expect(plain(occasions)).toContain('Виталий · День рождения');
  const details = screen.getByRole('region', { name: 'Нюансы' });
  expect(within(details).getByText('18,5–19,0')).toBeInTheDocument();
  expect(
    within(details).queryByRole('button', { name: /Действия/ }),
  ).toBeNull();
  expect(
    within(details).getByRole('link', { name: 'Изменить' }),
  ).toHaveAttribute('href', '/gifts/settings');
});

test('says when there are no wishes and no нюансы yet', async () => {
  renderSummary(() => json(empty));
  expect(await screen.findByText('Желаний пока нет')).toBeInTheDocument();
  expect(screen.getByText(/Нюансов пока нет/)).toBeInTheDocument();
  expect(screen.queryByRole('region', { name: '🔥 Очень хочу' })).toBeNull();
});

test('says why the summary did not load and retries', async () => {
  let fail = true;
  renderSummary(() =>
    fail ? json({ title: 'Сервер недоступен' }, 503) : json(summary),
  );
  expect(
    await screen.findByText(/Не удалось загрузить сводку: Сервер недоступен/),
  ).toBeInTheDocument();
  fail = false;
  await userEvent.click(screen.getByRole('button', { name: 'Повторить' }));
  expect(
    await screen.findByRole('region', { name: 'Маша' }),
  ).toBeInTheDocument();
});
