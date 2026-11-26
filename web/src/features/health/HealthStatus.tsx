import { useEffect, useState } from 'react';
import { getHealth, type Health } from './health-api';

type State =
  | { kind: 'loading' }
  | { kind: 'ready'; health: Health }
  | { kind: 'failed'; message: string };

export function HealthStatus() {
  const [state, setState] = useState<State>({ kind: 'loading' });

  useEffect(() => {
    const controller = new AbortController();
    getHealth(controller.signal)
      .then((health) => setState({ kind: 'ready', health }))
      .catch((error: unknown) => {
        if (controller.signal.aborted) return;
        const message = error instanceof Error ? error.message : String(error);
        setState({ kind: 'failed', message });
      });
    return () => controller.abort();
  }, []);

  if (state.kind === 'loading') return <p>Проверяю API…</p>;
  if (state.kind === 'failed')
    return <p role="alert">API недоступен: {state.message}</p>;

  const { health } = state;
  return (
    <section aria-label="Состояние API">
      <p>
        API: <strong>{health.status}</strong> ({health.environment}, версия{' '}
        {health.version})
      </p>
      {Object.entries(health.checks).length > 0 && (
        <ul>
          {Object.entries(health.checks).map(([name, status]) => (
            <li key={name}>
              {name}: {status}
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
