import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/client/query-client';
import { NotesSection } from '../section/NotesSection';
import { NotesPage } from './NotesPage';

afterEach(() => vi.unstubAllGlobals());

const categories = [
  {
    id: 'c-food',
    name: 'Їжа',
    emoji: '🍕',
    sortOrder: 10,
    version: 1,
    noteCount: 2,
  },
];

const note = (overrides: Record<string, unknown> = {}) => ({
  id: crypto.randomUUID(),
  about: 'Maria',
  kind: 'Likes',
  title: 'Нотатка',
  details: null,
  place: null,
  isImportant: false,
  categoryId: null,
  categoryName: null,
  categoryEmoji: null,
  createdBy: 'Maria',
  updatedBy: 'Maria',
  createdAtUtc: '2026-11-27T10:00:00Z',
  updatedAtUtc: '2026-11-27T10:00:00Z',
  version: 1,
  ...overrides,
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

function LocationProbe() {
  return <output data-testid="location">{useLocation().search}</output>;
}

/** Stubs the API: the member and the categories always answer; `answer` handles the notes. */
function renderPage(
  answer: (sent: Sent) => Promise<Response>,
  { path = '/notes', person = 'Vitaliy' as string | null } = {},
) {
  const sent: Sent[] = [];
  vi.stubGlobal('fetch', async (request: Request) => {
    const text = await request.text();
    const entry = {
      method: request.method,
      url: new URL(request.url),
      headers: request.headers,
      body: text ? JSON.parse(text) : undefined,
    };
    sent.push(entry);
    if (entry.url.pathname === '/api/identity/me')
      return json({
        email: 'a@example.com',
        name: 'A',
        pictureUrl: null,
        person,
      });
    if (entry.url.pathname === '/api/notes/categories') return json(categories);
    return answer(entry);
  });
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={[path]}>
          <Routes>
            <Route path="/notes" element={<NotesSection />}>
              <Route index element={<NotesPage />} />
            </Route>
          </Routes>
          <LocationProbe />
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  const lists = () =>
    sent.filter(
      (s) => s.method === 'GET' && s.url.pathname === '/api/notes/items',
    );
  const writes = () => sent.filter((s) => s.method !== 'GET');
  return { lists, writes };
}

const location = () => screen.getByTestId('location').textContent;

test('starts on «Про мене», groups the cards by category, and keeps the filters across the tabs (SC-001)', async () => {
  const { lists } = renderPage(
    () =>
      json([
        note({
          title: 'Піца',
          categoryId: 'c-food',
          categoryName: 'Їжа',
          categoryEmoji: '🍕',
          about: 'Vitaliy',
          isImportant: true,
          place: 'Celentano',
        }),
        note({
          title: 'Томатний сік',
          kind: 'Dislikes',
          about: 'Vitaliy',
          updatedBy: 'Vitaliy',
        }),
      ]),
    { path: '/notes?kind=likes' },
  );

  const tabs = await screen.findByRole('tablist', { name: 'Про кого' });
  expect(
    within(tabs)
      .getAllByRole('tab')
      .map((tab) => tab.textContent),
  ).toEqual(['Про мене', 'Про Машу', 'Усі']);
  expect(within(tabs).getByRole('tab', { name: 'Про мене' })).toHaveAttribute(
    'aria-current',
    'page',
  );
  const food = await screen.findByRole('region', { name: '🍕 Їжа · 1' });
  const card = within(food).getByRole('listitem');
  expect(card).toHaveTextContent('Любить');
  expect(card).toHaveTextContent('Піца');
  expect(card).toHaveTextContent('Celentano');
  expect(
    within(card).getByRole('img', { name: 'Важливо' }),
  ).toBeInTheDocument();
  expect(
    screen.getByRole('region', { name: 'Без категорії · 1' }),
  ).toHaveTextContent('Маша · 27.11 · змінив Віталій');
  expect(lists()[0]!.url.searchParams.get('about')).toBe('Vitaliy');
  expect(lists()[0]!.url.searchParams.getAll('kind')).toEqual(['Likes']);

  await userEvent.click(within(tabs).getByRole('tab', { name: 'Про Машу' }));
  await waitFor(() =>
    expect(lists().at(-1)!.url.searchParams.get('about')).toBe('Maria'),
  );
  expect(lists().at(-1)!.url.searchParams.getAll('kind')).toEqual(['Likes']);
  expect(location()).toContain('kind=likes');
  expect(within(tabs).getByRole('tab', { name: 'Про Машу' })).toHaveAttribute(
    'aria-current',
    'page',
  );
  expect(
    within(tabs).getByRole('tab', { name: 'Про мене' }),
  ).not.toHaveAttribute('aria-current');
  expect(screen.getByRole('textbox', { name: 'Нова нотатка' })).toHaveAttribute(
    'placeholder',
    'Що любить Маша?',
  );
});

test('a member without a person sees the people by name and starts on «Усі» with avatars', async () => {
  const { lists } = renderPage(
    () => json([note({ title: 'Каву', about: 'Vitaliy' })]),
    {
      person: null,
    },
  );

  const tabs = await screen.findByRole('tablist', { name: 'Про кого' });
  expect(
    within(tabs)
      .getAllByRole('tab')
      .map((tab) => tab.textContent),
  ).toEqual(['Віталій', 'Маша', 'Усі']);
  expect(within(tabs).getByRole('tab', { name: 'Усі' })).toHaveAttribute(
    'aria-selected',
    'true',
  );
  expect(
    await screen.findByRole('img', { name: 'Про Віталія' }),
  ).toBeInTheDocument();
  expect(lists()[0]!.url.searchParams.has('about')).toBe(false);
  expect(screen.getByRole('group', { name: 'Про кого' })).toBeInTheDocument();
});

test('quick add creates a note about the tab person with the chosen kind and an Idempotency-Key (SC-002)', async () => {
  const { writes } = renderPage(
    (s) => (s.method === 'POST' ? json({ id: 'n1' }, 201) : json([])),
    { path: '/notes?about=partner' },
  );

  expect(
    await screen.findByText('Про Машу ще нічого немає'),
  ).toBeInTheDocument();
  await userEvent.click(
    within(screen.getByRole('group', { name: 'Що' })).getByRole('button', {
      name: 'Не любить',
    }),
  );
  const field = screen.getByRole('textbox', { name: 'Нова нотатка' });
  expect(field).toHaveAttribute('placeholder', 'Що не любить Маша?');
  await userEvent.type(field, ' Томатний сік {Enter}');

  await waitFor(() => expect(field).toHaveValue(''));
  expect(field).toHaveFocus();
  expect(writes()[0]!.url.pathname).toBe('/api/notes/items');
  expect(writes()[0]!.headers.get('Idempotency-Key')).toBeTruthy();
  expect(writes()[0]!.body).toEqual({
    about: 'Maria',
    kind: 'Dislikes',
    title: 'Томатний сік',
    details: null,
    place: null,
    categoryId: null,
    isImportant: false,
  });
});

test('quick add keeps its Idempotency-Key on a retry and takes a new one for the next note', async () => {
  let fail = true;
  const { writes } = renderPage((s) => {
    if (s.method !== 'POST') return json([]);
    if (fail) {
      fail = false;
      return json({ title: 'Error', detail: 'Сервер недоступний.' }, 500);
    }
    return json({ id: 'n1' }, 201);
  });

  const field = await screen.findByRole('textbox', { name: 'Нова нотатка' });
  await userEvent.type(field, 'Каву{Enter}');
  expect(await screen.findByText(/Сервер недоступний/)).toBeInTheDocument();
  await userEvent.type(field, '{Enter}');
  await waitFor(() => expect(field).toHaveValue(''));
  await userEvent.type(field, 'Каву{Enter}');
  await waitFor(() => expect(writes()).toHaveLength(3));

  const keys = writes().map((w) => w.headers.get('Idempotency-Key'));
  expect(keys[1]).toBe(keys[0]);
  expect(keys[2]).not.toBe(keys[0]);
});

test('a filtered empty list resets the filters and keeps the tab', async () => {
  renderPage(() => json([]), { path: '/notes?about=all&kind=facts&q=сир' });

  await userEvent.click(
    await screen.findByRole('button', { name: 'Скинути фільтри' }),
  );

  await waitFor(() => expect(location()).toBe('?about=all'));
  expect(await screen.findByText('Тут ще нічого немає')).toBeInTheDocument();
});

test('a failed load says why and retries', async () => {
  let fail = true;
  renderPage(() => {
    if (fail) {
      fail = false;
      return json({ title: 'Error', detail: 'Сервер недоступний.' }, 500);
    }
    return json([note({ title: 'Квіти', about: 'Vitaliy' })]);
  });

  expect(await screen.findByText(/Сервер недоступний/)).toBeInTheDocument();
  await userEvent.click(screen.getByRole('button', { name: 'Повторити' }));
  expect(await screen.findByText('Квіти')).toBeInTheDocument();
});
