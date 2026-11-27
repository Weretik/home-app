import { Navigate, Route, Routes } from 'react-router-dom';
import { ExpensesPage } from '../../features/expenses/expense-list/ExpensesPage';
import { ForecastPage } from '../../features/expenses/forecast-page/ForecastPage';
import { IncomesPage } from '../../features/expenses/income-list/IncomesPage';
import { SavingsPage } from '../../features/expenses/savings-page/SavingsPage';
import { ExpensesSection } from '../../features/expenses/section/ExpensesSection';
import { SettingsPage } from '../../features/expenses/settings-page/SettingsPage';
import { GiftsSection } from '../../features/gifts/section/GiftsSection';
import { GiftSettingsPage } from '../../features/gifts/settings-page/GiftSettingsPage';
import { ArchivePage } from '../../features/gifts/pages/ArchivePage';
import { GiftsPage } from '../../features/gifts/pages/GiftsPage';
import { SessionGate } from '../../features/identity/sign-in/SessionGate';
import { HomePage } from '../../features/home/page/HomePage';
import { HistoryPage } from '../../features/shopping/history-page/HistoryPage';
import { ListPage } from '../../features/shopping/list-page/ListPage';
import { ShoppingSection } from '../../features/shopping/section/ShoppingSection';
import { SettingsPage as ShoppingSettingsPage } from '../../features/shopping/settings-page/SettingsPage';
import { NotesPage } from '../../features/notes/note-list/NotesPage';
import { NotesSection } from '../../features/notes/section/NotesSection';
import { CategoriesSettingsPage as NoteCategoriesPage } from '../../features/notes/settings/CategoriesSettingsPage';
import { NotificationsPage } from '../../features/notifications/page/NotificationsPage';
import { StatusPage } from '../../features/status/StatusPage';
import { ArchivePage as TodosArchivePage } from '../../features/todos/archive/ArchivePage';
import { CategoriesSettingsPage } from '../../features/todos/settings/CategoriesSettingsPage';
import { TodosSection } from '../../features/todos/section/TodosSection';
import { ActiveTasksPage } from '../../features/todos/task-list/ActiveTasksPage';
import { AppLayout } from '../shell/AppLayout';

/** Every route sits behind the sign-in gate; there is no /sign-in route, so the URL survives sign-in. */
export function App() {
  return (
    <SessionGate>
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
          <Route path="/shopping" element={<ShoppingSection />}>
            <Route index element={<ListPage />} />
            <Route path="history" element={<HistoryPage />} />
            <Route path="settings" element={<ShoppingSettingsPage />} />
          </Route>
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
          <Route path="/notes" element={<NotesSection />}>
            <Route index element={<NotesPage />} />
            <Route path="settings" element={<NoteCategoriesPage />} />
          </Route>
          <Route path="/status" element={<StatusPage />} />
          <Route path="/notifications" element={<NotificationsPage />} />
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </SessionGate>
  );
}
