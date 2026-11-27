import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/query-client';
import type { GiftItem } from '../data/gifts-api';
import { GiftsSection } from '../section/GiftsSection';
import { ArchivePage } from './ArchivePage';
import { GiftsPage } from './GiftsPage';

afterEach(() => vi.unstubAllGlobals());

const categories = [{ id: 'c-jewel', name: 'Украшения', sortOrder: 70 }];

const gift = (overrides: Partial<GiftItem> = {}): GiftItem => ({
  id: crypto.randomUUID(),
  version: 7,
  recipient: 'Maria',
  name: 'Серьги',
  categoryId: 'c-jewel',
  categoryName: 'Украшения',
  priority: 'High',
  price: null,
  link: null,
  status: 'Wanted',
  giftedOn: null,
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

function LocationProbe() {
  const location = useLocation();
  return (
    <output data-testid="location">
      {location.pathname + location.search}
    </output>
  );
}
const where = () => screen.getByTestId('location').textContent ?? '';

/** Stubs the API with a wanted list and an archive; records the writes. */
function renderAt(path: string, wanted: GiftItem[], archived: GiftItem[] = []) {
  const writes: { method: string; body: unknown }[] = [];
  vi.stubGlobal(
    'fetch',
    vi.fn(async (input: Request) => {
      const url = new URL(input.url);
      if (url.pathname === '/api/gifts/categories') return json(categories);
      if (input.method === 'GET')
        return json(
          url.searchParams.get('archived') === 'true' ? archived : wanted,
        );
      const text = await input.text();
      writes.push({
        method: input.method,
        body: text ? JSON.parse(text) : undefined,
      });
      return json(undefined, 204);
    }),
  );
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={[path]}>
          <Routes>
            <Route path="/gifts" element={<GiftsSection />}>
              <Route index element={<GiftsPage />} />
              <Route path="archive" element={<ArchivePage />} />
            </Route>
          </Routes>
          <LocationProbe />
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  return writes;
}

test('a link to a wanted gift opens its dialog and leaves the link', async () => {
  const ring = gift({ name: 'Кольцо' });
  renderAt(`/gifts?gift=${ring.id}`, [gift(), ring]);

  const dialog = await screen.findByRole('dialog', {
    name: 'Змінити подарунок',
  });
  expect(within(dialog).getByLabelText('Назва')).toHaveValue('Кольцо');
  await waitFor(() => expect(where()).toBe('/gifts'));
});

test('a link to a given gift moves to «Подаровані» and opens it there', async () => {
  const given = gift({
    name: 'Плед',
    status: 'Gifted',
    giftedOn: '2026-11-20',
  });
  renderAt(`/gifts?gift=${given.id}`, [gift()], [given]);

  const dialog = await screen.findByRole('dialog', {
    name: 'Змінити подарунок',
  });
  expect(within(dialog).getByLabelText('Назва')).toHaveValue('Плед');
  await waitFor(() => expect(where()).toBe('/gifts/archive'));
});

test('a link to a gift nobody has says so', async () => {
  renderAt('/gifts?gift=missing', [gift()], []);

  expect(await screen.findByText(/Подарунок не знайдено/)).toBeInTheDocument();
  expect(where()).toBe('/gifts/archive?gift=missing');
});

test('a wanted gift shows when to buy it and who gives it', async () => {
  renderAt('/gifts', [
    gift({ name: 'Духи', giver: 'Vitaliy', buyBy: '2099-12-12' }),
  ]);

  expect(
    await screen.findAllByText('🛒 купити до 12.12 · дарує Віталій'),
  ).not.toHaveLength(0);
});

test('the dialog sends who gives the gift and the buy-by day', async () => {
  const user = userEvent.setup();
  const perfume = gift({ name: 'Духи' });
  const writes = renderAt(`/gifts?gift=${perfume.id}`, [perfume]);

  const dialog = await screen.findByRole('dialog', {
    name: 'Змінити подарунок',
  });
  await user.click(within(dialog).getByLabelText('Хто дарує'));
  await user.click(await screen.findByRole('option', { name: 'Віталій' }));
  await user.type(within(dialog).getByLabelText('Купити до'), '2026-12-20');
  expect(
    within(dialog).getByText(/Завдання з’явиться у «Справах»/),
  ).toBeInTheDocument();
  await user.click(within(dialog).getByRole('button', { name: 'Зберегти' }));

  await waitFor(() => expect(writes).toHaveLength(1));
  expect(writes[0]?.body).toMatchObject({
    giver: 'Vitaliy',
    buyBy: '2026-12-20',
  });
});
