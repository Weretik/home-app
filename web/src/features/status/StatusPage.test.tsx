import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import { afterEach, vi } from 'vitest';
import { createQueryClient } from '../../platform/query-client';
import { StatusPage } from './StatusPage';

afterEach(() => vi.unstubAllGlobals());

function renderPage(response: () => Promise<Response>) {
  vi.stubGlobal('fetch', vi.fn().mockImplementation(response));
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <QueryClientProvider client={client}>
      <StatusPage />
    </QueryClientProvider>,
  );
}

const json = (body: unknown, status = 200) =>
  Promise.resolve(
    new Response(JSON.stringify(body), {
      status,
      headers: { 'Content-Type': 'application/json' },
    }),
  );

test('shows the API status and each check', async () => {
  renderPage(() =>
    json({
      status: 'Healthy',
      version: '1.0.0',
      environment: 'Production',
      checks: { database: 'Healthy' },
    }),
  );

  expect(await screen.findByRole('alert')).toHaveTextContent('API: Healthy');
  expect(screen.getByText('База данных')).toBeInTheDocument();
});

test('says when the API cannot be reached', async () => {
  renderPage(() => Promise.reject(new TypeError('Failed to fetch')));

  expect(await screen.findByRole('alert')).toHaveTextContent('Failed to fetch');
});
