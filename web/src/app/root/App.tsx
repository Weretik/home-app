import { Navigate, Route, Routes } from 'react-router-dom';
import { ExpensesPage } from '../../features/expenses/expense-list/ExpensesPage';
import { IncomesPage } from '../../features/expenses/income-list/IncomesPage';
import { SavingsPage } from '../../features/expenses/savings-page/SavingsPage';
import { ExpensesSection } from '../../features/expenses/section/ExpensesSection';
import { SettingsPage } from '../../features/expenses/settings-page/SettingsPage';
import { GiftsSection } from '../../features/gifts/section/GiftsSection';
import { GiftSettingsPage } from '../../features/gifts/settings-page/GiftSettingsPage';
import { ArchivePage } from '../../features/gifts/pages/ArchivePage';
import { GiftsPage } from '../../features/gifts/pages/GiftsPage';
import { ComingSoonPage } from '../../features/home/page/ComingSoonPage';
import { HomePage } from '../../features/home/page/HomePage';
import { StatusPage } from '../../features/status/StatusPage';
import { ArchivePage as TodosArchivePage } from '../../features/todos/archive/ArchivePage';
import { CategoriesSettingsPage } from '../../features/todos/settings/CategoriesSettingsPage';
import { TodosSection } from '../../features/todos/section/TodosSection';
import { ActiveTasksPage } from '../../features/todos/task-list/ActiveTasksPage';
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
          <Route path="settings" element={<SettingsPage />} />
        </Route>
        <Route path="/shopping" element={<ComingSoonPage title="Покупки" />} />
        <Route path="/gifts" element={<GiftsSection />}>
          <Route index element={<GiftsPage />} />
          <Route path="archive" element={<ArchivePage />} />
          <Route path="settings" element={<GiftSettingsPage />} />
        </Route>
        <Route path="/todos" element={<TodosSection />}>
          <Route index element={<ActiveTasksPage />} />
          <Route path="archive" element={<TodosArchivePage />} />
          <Route path="settings" element={<CategoriesSettingsPage />} />
        </Route>
        <Route path="/status" element={<StatusPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
