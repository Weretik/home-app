import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/query-client';
import { SettingsPage } from './SettingsPage';

afterEach(() => vi.unstubAllGlobals());

const departments = [
  {
    id: 'd-dairy',
    name: 'Молочка',
    icon: '🥛',
    sortOrder: 1,
    isFallback: false,
    keywordCount: 12,
    neededCount: 2,
    version: 5,
  },
  {
    id: 'd-bread',
    name: 'Хлеб',
    icon: '🍞',
    sortOrder: 2,
    isFallback: false,
    keywordCount: 1,
    neededCount: 0,
    version: 2,
  },
  {
    id: 'd-other',
    name: 'Другое',
    icon: '🧺',
    sortOrder: 3,
    isFallback: true,
    keywordCount: 0,
    neededCount: 0,
    version: 1,
  },
];
const keywords = [
  {
    id: 'k1',
    keyword: 'молок',
    departmentId: 'd-dairy',
    departmentName: 'Молочка',
    departmentIcon: '🥛',
    learned: false,
    version: 1,
  },
  {
    id: 'k2',
    keyword: 'сырок глазир',
    departmentId: 'd-dairy',
    departmentName: 'Молочка',
    departmentIcon: '🥛',
    learned: true,
    version: 4,
  },
];

const json = (body: unknown, status = 200) =>
  Promise.resolve(
    new Response(JSON.stringify(body), {
      status,
      headers: { 'Content-Type': 'application/json' },
    }),
  );

function renderSettings() {
  const sent: { method: string; url: URL; headers: Headers; body: unknown }[] =
    [];
  vi.stubGlobal('fetch', async (request: Request) => {
    const text = await request.text();
    const url = new URL(request.url);
    sent.push({
      method: request.method,
      url,
      headers: request.headers,
      body: text ? JSON.parse(text) : undefined,
    });
    if (request.method === 'GET') {
      if (url.pathname === '/api/shopping/departments')
        return json(departments);
      if (url.pathname === '/api/shopping/keywords') return json(keywords);
      return json({ cartKeepHours: 3, frequentDays: 183, version: 7 });
    }
    if (request.method === 'POST') return json({ id: 'new' }, 201);
    return new Response(null, { status: 204 });
  });
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter>
          <SettingsPage />
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  return { writes: () => sent.filter((s) => s.method !== 'GET'), sent };
}

test('lists departments in order with counts; the fallback cannot be deleted', async () => {
  renderSettings();

  const list = await screen.findByRole('list', { name: 'Отделы' });
  const rows = await within(list).findAllByRole('listitem');
  expect(rows).toHaveLength(3);
  expect(rows[0]).toHaveTextContent('🥛 Молочка');
  expect(rows[0]).toHaveTextContent('12 слов · 2 в списке');
  expect(rows[1]).toHaveTextContent('1 слово');
  expect(
    within(rows[0]!).getByRole('button', { name: 'Выше: Молочка' }),
  ).toBeDisabled();

  await userEvent.click(
    within(rows[2]!).getByRole('button', { name: 'Действия: Другое' }),
  );
  expect(
    screen.queryByRole('menuitem', { name: 'Удалить' }),
  ).not.toBeInTheDocument();
});

test('moves a department up and adds a new one', async () => {
  const { writes } = renderSettings();
  await screen.findByText('🍞 Хлеб');

  await userEvent.click(screen.getByRole('button', { name: 'Выше: Хлеб' }));
  await waitFor(() => expect(writes()).toHaveLength(1));
  expect(writes()[0]!.url.pathname).toBe('/api/shopping/departments/order');
  expect(writes()[0]!.body).toEqual({
    departmentIds: ['d-bread', 'd-dairy', 'd-other'],
  });

  await userEvent.click(screen.getByRole('button', { name: 'Добавить отдел' }));
  const dialog = await screen.findByRole('dialog', { name: 'Новый отдел' });
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );
  expect(within(dialog).getByText('Введите название')).toBeInTheDocument();
  await userEvent.type(within(dialog).getByLabelText(/Название/), 'Зоотовары');
  await userEvent.type(within(dialog).getByLabelText('Значок'), '🐱');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );
  await waitFor(() =>
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument(),
  );
  expect(writes()[1]!.headers.get('Idempotency-Key')).toBeTruthy();
  expect(writes()[1]!.body).toEqual({ name: 'Зоотовары', icon: '🐱' });
});

test('deletes a department after asking, with its version', async () => {
  const { writes } = renderSettings();
  await screen.findByText('🍞 Хлеб');

  await userEvent.click(screen.getByRole('button', { name: 'Действия: Хлеб' }));
  await userEvent.click(screen.getByRole('menuitem', { name: 'Удалить' }));
  const dialog = await screen.findByRole('dialog', { name: 'Удалить отдел?' });
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Удалить' }),
  );
  await waitFor(() => expect(writes()).toHaveLength(1));
  expect(writes()[0]!.method).toBe('DELETE');
  expect(writes()[0]!.url.pathname).toBe('/api/shopping/departments/d-bread');
  expect(writes()[0]!.headers.get('If-Match')).toBe('2');
});

test('searches keywords, marks learned ones and edits one with its version', async () => {
  const { writes, sent } = renderSettings();
  const list = await screen.findByRole('list', { name: 'Слова для отделов' });
  expect((await within(list).findAllByRole('listitem'))[1]).toHaveTextContent(
    'выучено',
  );

  await userEvent.type(screen.getByLabelText('Поиск'), 'сыр');
  await waitFor(() =>
    expect(sent.some((s) => s.url.searchParams.get('search') === 'сыр')).toBe(
      true,
    ),
  );

  await userEvent.click(
    screen.getByRole('button', { name: 'Действия: сырок глазир' }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Изменить' }));
  const dialog = await screen.findByRole('dialog', { name: 'Изменить слово' });
  const word = within(dialog).getByLabelText(/Слово/);
  await userEvent.clear(word);
  await userEvent.type(word, 'Сырок  Глазированный');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );
  await waitFor(() => expect(writes()).toHaveLength(1));
  expect(writes()[0]!.url.pathname).toBe('/api/shopping/keywords/k2');
  expect(writes()[0]!.headers.get('If-Match')).toBe('4');
  expect(writes()[0]!.body).toEqual({
    keyword: 'сырок глазированный',
    departmentId: 'd-dairy',
  });
});

test('checks and saves the list settings with the version', async () => {
  const { writes } = renderSettings();
  const hours = await screen.findByLabelText(
    'Сколько часов купленное лежит в корзине',
  );
  expect(hours).toHaveValue('3');

  await userEvent.clear(hours);
  await userEvent.type(hours, '100');
  await userEvent.click(screen.getByRole('button', { name: 'Сохранить' }));
  expect(screen.getByText('От 1 до 72')).toBeInTheDocument();
  expect(writes()).toHaveLength(0);

  await userEvent.clear(hours);
  await userEvent.type(hours, '6');
  await userEvent.click(screen.getByRole('button', { name: 'Сохранить' }));
  expect(await screen.findByText('Сохранено')).toBeInTheDocument();
  expect(writes()[0]!.headers.get('If-Match')).toBe('7');
  expect(writes()[0]!.body).toEqual({ cartKeepHours: 6, frequentDays: 183 });
});

test('adds a keyword with an Idempotency-Key and the filtered department', async () => {
  const { writes } = renderSettings();
  await screen.findByText('🍞 Хлеб');

  await userEvent.click(screen.getByRole('button', { name: 'Добавить слово' }));
  const dialog = await screen.findByRole('dialog', { name: 'Новое слово' });
  await userEvent.type(within(dialog).getByLabelText(/Слово/), 'Ё');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );
  expect(within(dialog).getByText('Не короче 2 букв')).toBeInTheDocument();
  expect(within(dialog).getByText('Выберите отдел')).toBeInTheDocument();

  await userEvent.type(within(dialog).getByLabelText(/Слово/), 'жик');
  await userEvent.click(
    within(dialog).getByRole('combobox', { name: /Отдел/ }),
  );
  await userEvent.click(await screen.findByRole('option', { name: /Хлеб/ }));
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );
  await waitFor(() => expect(writes()).toHaveLength(1));
  expect(writes()[0]!.url.pathname).toBe('/api/shopping/keywords');
  expect(writes()[0]!.headers.get('Idempotency-Key')).toBeTruthy();
  expect(writes()[0]!.body).toEqual({
    keyword: 'ежик',
    departmentId: 'd-bread',
  });
});
