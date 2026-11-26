import { Navigate, Route, Routes } from 'react-router-dom';
import { ExpensesPage } from '../../features/expenses/expense-list/ExpensesPage';
import { ForecastPage } from '../../features/expenses/forecast-page/ForecastPage';
import { IncomesPage } from '../../features/expenses/income-list/IncomesPage';
import { SavingsPage } from '../../features/expenses/savings-page/SavingsPage';
import { ExpensesSection } from '../../features/expenses/section/ExpensesSection';
import { SettingsPage } from '../../features/expenses/settings-page/SettingsPage';
import { ComingSoonPage } from '../../features/home/page/ComingSoonPage';
import { HomePage } from '../../features/home/page/HomePage';
import { StatusPage } from '../../features/status/StatusPage';
import { AppLayout } from '../shell/AppLayout';

export function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route index element={<HomePage />} />
        <Route path="/expenses" element={<ExpensesSection />}>
          <Route index element={<ExpensesPage />} />
          <Route path="incomes" element={<IncomesPage />} />
          <Route path="savings" element={<SavingsPage />} />
          <Route path="forecast" element={<ForecastPage />} />
          <Route path="settings" element={<SettingsPage />} />
        </Route>
        <Route path="/shopping" element={<ComingSoonPage title="Покупки" />} />
        <Route path="/todos" element={<ComingSoonPage title="Дела" />} />
        <Route path="/gifts" element={<ComingSoonPage title="Подарки" />} />
        <Route path="/status" element={<StatusPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
