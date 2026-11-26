import { render, screen } from '@testing-library/react';
import { afterEach, vi } from 'vitest';
import { HealthStatus } from './HealthStatus';

afterEach(() => vi.unstubAllGlobals());

function stubFetch(status: number, body: unknown) {
  vi.stubGlobal(
    'fetch',
    vi.fn().mockResolvedValue(new Response(JSON.stringify(body), { status })),
  );
}

test('shows the API status and its checks', async () => {
  stubFetch(200, {
    status: 'Healthy',
    version: '1.0.0',
    environment: 'Production',
    checks: { database: 'Healthy' },
  });

  render(<HealthStatus />);

  expect(await screen.findByText('Healthy')).toBeInTheDocument();
  expect(screen.getByText('database: Healthy')).toBeInTheDocument();
});

test('shows an unhealthy API instead of an error', async () => {
  stubFetch(503, {
    status: 'Unhealthy',
    version: '1.0.0',
    environment: 'Production',
    checks: { database: 'Unhealthy' },
  });

  render(<HealthStatus />);

  expect(await screen.findByText('Unhealthy')).toBeInTheDocument();
});

test('says when the API cannot be reached', async () => {
  vi.stubGlobal(
    'fetch',
    vi.fn().mockRejectedValue(new TypeError('Failed to fetch')),
  );

  render(<HealthStatus />);

  expect(await screen.findByRole('alert')).toHaveTextContent('Failed to fetch');
});
