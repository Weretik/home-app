import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/query-client';
import { NotesPage } from '../note-list/NotesPage';
import { NotesSection } from '../section/NotesSection';

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

const noContent = () => Promise.resolve(new Response(null, { status: 204 }));
const problem = (status: number, code: string) =>
  json({ title: 'Problem', detail: 'English detail.', code }, status);

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

const juice = note({
  id: 'n-juice',
  title: 'Томатний сік',
  kind: 'Dislikes',
  about: 'Vitaliy',
  version: 4,
});

async function openJuice() {
  await userEvent.click(
    await screen.findByRole('button', { name: /Томатний сік/ }),
  );
  return screen.findByRole('dialog', { name: 'Нотатка' });
}

test('changes a note with its version in If-Match (SC-003)', async () => {
  const { writes } = renderPage((s) =>
    s.method === 'PUT' ? noContent() : json([juice]),
  );

  const dialog = await openJuice();
  const title = within(dialog).getByRole('textbox', { name: /Що саме/ });
  await userEvent.clear(title);
  await userEvent.type(title, 'Томатний сік у пакетах');
  await userEvent.type(
    within(dialog).getByRole('textbox', { name: 'Де купити / бренд' }),
    'Сільпо',
  );
  await userEvent.click(
    within(dialog).getByRole('switch', { name: 'Важливо' }),
  );
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await waitFor(() =>
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument(),
  );

  expect(writes()[0]!.url.pathname).toBe('/api/notes/items/n-juice');
  expect(writes()[0]!.headers.get('If-Match')).toBe('4');
  expect(writes()[0]!.body).toEqual({
    about: 'Vitaliy',
    kind: 'Dislikes',
    title: 'Томатний сік у пакетах',
    details: null,
    place: 'Сільпо',
    categoryId: null,
    isImportant: true,
  });
});

test('a 409 reloads the list, shows the note as it is now and saves with the new version (SC-004)', async () => {
  let current = juice;
  let conflict = true;
  const { writes } = renderPage((s) => {
    if (s.method === 'PUT') {
      if (conflict) {
        conflict = false;
        current = { ...juice, title: 'Сік змінила Маша', version: 5 };
        return problem(409, 'notes.changed_meanwhile');
      }
      return noContent();
    }
    return json([current]);
  });

  const dialog = await openJuice();
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );

  expect(
    await within(dialog).findByText(/Нотатку змінили в іншому місці/),
  ).toBeInTheDocument();
  await waitFor(() =>
    expect(
      within(dialog).getByRole('textbox', { name: /Що саме/ }),
    ).toHaveValue('Сік змінила Маша'),
  );
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await waitFor(() =>
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument(),
  );
  expect(writes()[1]!.headers.get('If-Match')).toBe('5');
});

test('deletes a note after asking', async () => {
  const { writes } = renderPage((s) =>
    s.method === 'DELETE' ? noContent() : json([juice]),
  );

  const dialog = await openJuice();
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Видалити' }),
  );
  const confirm = await screen.findByRole('dialog', {
    name: 'Видалити нотатку?',
  });
  expect(confirm).toHaveTextContent('Томатний сік');
  await userEvent.click(
    within(confirm).getByRole('button', { name: 'Видалити' }),
  );
  await waitFor(() =>
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument(),
  );

  expect(writes()[0]!.method).toBe('DELETE');
  expect(writes()[0]!.headers.get('If-Match')).toBe('4');
});

test('a delete answered 409 reloads the form, so a save then keeps the other change in view (ADR 0007)', async () => {
  let current = juice;
  const { writes } = renderPage((s) => {
    if (s.method === 'DELETE') {
      current = { ...juice, title: 'Сік змінила Маша', version: 5 };
      return problem(409, 'notes.changed_meanwhile');
    }
    if (s.method === 'PUT') return noContent();
    return json([current]);
  });

  const dialog = await openJuice();
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Видалити' }),
  );
  const confirm = await screen.findByRole('dialog', {
    name: 'Видалити нотатку?',
  });
  await userEvent.click(
    within(confirm).getByRole('button', { name: 'Видалити' }),
  );
  expect(
    await within(confirm).findByText(/Нотатку змінили в іншому місці/),
  ).toBeInTheDocument();
  await userEvent.click(
    within(confirm).getByRole('button', { name: 'Скасувати' }),
  );
  await waitFor(() =>
    expect(
      within(dialog).getByRole('textbox', { name: /Що саме/ }),
    ).toHaveValue('Сік змінила Маша'),
  );
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await waitFor(() =>
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument(),
  );
  expect(writes()[1]!.headers.get('If-Match')).toBe('5');
  expect(writes()[1]!.body).toMatchObject({ title: 'Сік змінила Маша' });
});

test('a note deleted elsewhere closes the dialog and says so', async () => {
  renderPage((s) =>
    s.method === 'PUT' ? problem(404, 'notes.note_not_found') : json([juice]),
  );

  const dialog = await openJuice();
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );

  expect(await screen.findByText(/Цієї нотатки вже немає/)).toBeInTheDocument();
  await waitFor(() =>
    expect(
      screen.queryByRole('dialog', { name: 'Нотатка' }),
    ).not.toBeInTheDocument(),
  );
});

test('the pencil opens a new note with the typed text; it saves with an Idempotency-Key and a category', async () => {
  const { writes } = renderPage((s) =>
    s.method === 'POST' ? json({ id: 'n2' }, 201) : json([]),
  );

  const field = await screen.findByRole('textbox', { name: 'Нова нотатка' });
  await userEvent.type(field, 'Піца');
  await userEvent.click(screen.getByRole('button', { name: 'Детальна форма' }));
  const dialog = await screen.findByRole('dialog', { name: 'Нова нотатка' });
  expect(within(dialog).getByRole('textbox', { name: /Що саме/ })).toHaveValue(
    'Піца',
  );
  await userEvent.click(within(dialog).getByRole('button', { name: 'Маша' }));
  await userEvent.click(
    within(dialog).getByRole('combobox', { name: 'Категорія' }),
  );
  await userEvent.click(await screen.findByRole('option', { name: '🍕 Їжа' }));
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await waitFor(() =>
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument(),
  );

  expect(writes()[0]!.headers.get('Idempotency-Key')).toBeTruthy();
  expect(writes()[0]!.body).toMatchObject({
    about: 'Maria',
    kind: 'Likes',
    title: 'Піца',
    categoryId: 'c-food',
  });
});
