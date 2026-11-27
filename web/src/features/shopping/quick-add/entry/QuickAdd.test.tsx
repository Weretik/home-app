import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../../app/theme/theme';
import { createQueryClient } from '../../../../platform/client/query-client';
import { QuickAdd } from './QuickAdd';
import { useQuickAdd } from './use-quick-add';

afterEach(() => {
  vi.unstubAllGlobals();
  localStorage.clear();
});

const json = (body: unknown, status = 200) =>
  Promise.resolve(
    new Response(JSON.stringify(body), {
      status,
      headers: { 'Content-Type': 'application/json' },
    }),
  );

const bread = {
  name: 'Хлеб',
  departmentId: 'd-bread',
  departmentName: 'Хлеб',
  departmentIcon: '🍞',
  timesBought: 3,
  lastAddedAtUtc: '2026-11-20T10:00:00Z',
  inList: true,
};
const loaf = {
  ...bread,
  name: 'Хлебцы ржаные',
  departmentName: 'Бакалея',
  timesBought: 0,
  inList: false,
};

function Harness() {
  return <QuickAdd quickAdd={useQuickAdd()} />;
}

function renderQuickAdd() {
  const sent: { method: string; url: URL; body: unknown }[] = [];
  vi.stubGlobal('fetch', async (request: Request) => {
    const text = await request.text();
    const url = new URL(request.url);
    sent.push({
      method: request.method,
      url,
      body: text ? JSON.parse(text) : undefined,
    });
    if (url.pathname === '/api/shopping/items/suggestions')
      return json([bread, loaf]);
    if (url.pathname === '/api/shopping/frequent') return json([]);
    if (url.pathname === '/api/shopping/departments/match')
      return json({
        departmentId: 'd-bread',
        departmentName: 'Хлеб',
        departmentIcon: '🍞',
      });
    if (request.method === 'POST')
      return json({ added: [], restored: [], updated: [] }, 201);
    return new Response(null, { status: 204 });
  });
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <Harness />
      </QueryClientProvider>
    </ThemeProvider>,
  );
  return {
    field: screen.getByRole('combobox', { name: 'Що купити' }),
    asked: () =>
      sent
        .filter((s) => s.url.pathname === '/api/shopping/items/suggestions')
        .map((s) => s.url.searchParams.toString()),
    posts: () => sent.filter((s) => s.method === 'POST'),
  };
}

test('suggests names for the segment being typed, with department, count and «вже в списку»', async () => {
  const { field, asked } = renderQuickAdd();

  await userEvent.type(field, 'молоко, хле');

  const options = await screen.findAllByRole('option');
  expect(options).toHaveLength(2);
  expect(options[0]).toHaveTextContent('Хлеб');
  expect(options[0]).toHaveTextContent('Хлеб · брали 3 рази');
  expect(within(options[0]!).getByText('вже в списку')).toBeInTheDocument();
  expect(options[1]).toHaveTextContent('Бакалея · ще не брали');
  expect(options[1]).not.toHaveTextContent('вже в списку');
  // Asked once, after the pause, for the last segment only.
  expect(asked()).toEqual([`query=${encodeURIComponent('хле')}&limit=8`]);
});

test('choosing a suggestion replaces only the current segment and adds nothing', async () => {
  const { field, posts } = renderQuickAdd();
  await userEvent.type(field, 'молоко,хле');

  await userEvent.click(
    await screen.findByRole('option', { name: /Хлебцы ржаные/ }),
  );

  expect(field).toHaveValue('молоко, Хлебцы ржаные');
  expect(posts()).toHaveLength(0);
  await waitFor(() =>
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument(),
  );
});

test('arrow keys pick a suggestion with Enter; the next Enter adds the text', async () => {
  const { field, posts } = renderQuickAdd();
  await userEvent.type(field, 'хле');
  await screen.findAllByRole('option');

  await userEvent.keyboard('{ArrowDown}{Enter}');
  expect(field).toHaveValue('Хлеб');
  expect(posts()).toHaveLength(0);

  await userEvent.type(field, ' 2{Enter}');
  await waitFor(() => expect(posts()).toHaveLength(1));
  expect(posts()[0]!.body).toMatchObject({ text: 'Хлеб 2' });
});

test('Enter without a highlighted suggestion adds what was typed', async () => {
  const { field, posts } = renderQuickAdd();
  await userEvent.type(field, 'хле');
  await screen.findAllByRole('option');

  await userEvent.keyboard('{Enter}');

  await waitFor(() => expect(posts()).toHaveLength(1));
  expect(posts()[0]!.body).toMatchObject({ text: 'хле' });
});

test('an empty segment asks for nothing and hides the list', async () => {
  const { field, asked } = renderQuickAdd();
  await userEvent.type(field, 'молоко');
  await screen.findAllByRole('option');

  await userEvent.type(field, ', ');
  await new Promise((resolve) => setTimeout(resolve, 300));

  expect(asked()).toEqual([`query=${encodeURIComponent('молоко')}&limit=8`]);
  expect(screen.queryByRole('option')).not.toBeInTheDocument();
});
