import { HealthStatus } from '../features/health/HealthStatus';

export function App() {
  return (
    <main>
      <h1>Дом</h1>
      <p>
        Расходы, покупки, дела и подарки. Пока здесь только проверка связи с
        API.
      </p>
      <HealthStatus />
    </main>
  );
}
