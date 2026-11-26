import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/query-client';
import type { GiftItem } from '../data/gifts-api';
import { ArchivePage } from './ArchivePage';

afterEach(() => vi.unstubAllGlobals());

const gift = (overrides: Partial<GiftItem>): GiftItem => ({
  id: crypto.randomUUID(),
  version: 2,
  recipient: 'Maria',
  name: 'Серьги',
  categoryId: 'c1',
  categoryName: 'Украшения',
  priority: 'High',
  price: 4500,
  link: null,
  status: 'Gifted',
  giftedOn: '2026-11-26',
  occasion: null,
  occasionDate: null,
  giver: null,
  buyBy: null,
  notes: null,
  variants: [],
  createdAtUtc: '2026-11-01T00:00:00Z',
  ...overrides,
});

const json = (body: unknown, status = 200) =>
  Promise.resolve(
    new Response(body === undefined ? null : JSON.stringify(body), {
      status,
      headers: { 'Content-Type': 'application/json' },
    }),
  );

function renderArchive(items: () => GiftItem[], onWrite?: () => void) {
  const sent: { method: string; url: URL; body: unknown }[] = [];
  vi.stubGlobal(
    'fetch',
    vi.fn(async (input: Request) => {
      const text = await input.text();
      const url = new URL(input.url);
      sent.push({
        method: input.method,
        url,
        body: text ? JSON.parse(text) : undefined,
      });
      if (url.pathname === '/api/gifts/categories')
        return json([{ id: 'c1', name: 'Украшения', sortOrder: 1 }]);
      if (input.method === 'GET') return json(items());
      onWrite?.();
      return json(undefined, 204);
    }),
  );
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={['/gifts/archive']}>
          <Routes>
            <Route path="/gifts/archive" element={<ArchivePage />} />
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  return sent;
}

test('shows gifted and dropped gifts in a block per person, with how each ended', async () => {
  const sent = renderArchive(() => [
    gift({ name: 'Серьги' }),
    gift({
      name: 'Плед',
      recipient: 'Together',
      status: 'Dropped',
      giftedOn: null,
    }),
  ]);
  const masha = await screen.findByRole('region', { name: 'Маша' });
  expect(within(masha).getByText('Серьги')).toBeInTheDocument();
  expect(
    within(masha).getByText('Подарено 26 ноября 2026'),
  ).toBeInTheDocument();
  const together = screen.getByRole('region', { name: 'Вместе' });
  expect(within(together).getByText('Плед')).toBeInTheDocument();
  expect(within(together).getByText('Не актуально')).toBeInTheDocument();
  const list = sent.find((s) => s.url.pathname === '/api/gifts/items')!;
  expect(list.url.searchParams.get('archived')).toBe('true');
});

test('says when nothing was gifted yet', async () => {
  renderArchive(() => []);
  expect(
    await screen.findByText('Здесь появятся подаренные подарки'),
  ).toBeInTheDocument();
});

test('returns a gift to the list', async () => {
  let back = false;
  const item = gift({ name: 'Серьги' });
  const sent = renderArchive(
    () => (back ? [] : [item]),
    () => {
      back = true;
    },
  );
  await userEvent.click(
    await screen.findByRole('button', { name: 'Действия: Серьги' }),
  );
  await userEvent.click(
    screen.getByRole('menuitem', { name: 'Вернуть в список' }),
  );
  expect(
    await screen.findByText('Здесь появятся подаренные подарки'),
  ).toBeInTheDocument();
  const put = sent.find((s) => s.method === 'PUT')!;
  expect(put.url.pathname).toBe(`/api/gifts/items/${item.id}/status`);
  expect(put.body).toEqual({ status: 'Wanted', giftedOn: null });
  await waitFor(() =>
    expect(screen.queryAllByRole('listitem')).toHaveLength(0),
  );
});
