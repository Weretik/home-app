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
import { HistoryPage } from '../../features/shopping/history-page/HistoryPage';
import { ListPage } from '../../features/shopping/list-page/ListPage';
import { ShoppingSection } from '../../features/shopping/section/ShoppingSection';
import { SettingsPage as ShoppingSettingsPage } from '../../features/shopping/settings-page/SettingsPage';
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
          <Route path="settings" element={<SettingsPage />} />
        </Route>
        <Route path="/shopping" element={<ShoppingSection />}>
          <Route index element={<ListPage />} />
          <Route path="history" element={<HistoryPage />} />
          <Route path="settings" element={<ShoppingSettingsPage />} />
        </Route>
        <Route path="/todos" element={<ComingSoonPage title="Дела" />} />
        <Route path="/gifts" element={<GiftsSection />}>
          <Route index element={<GiftsPage />} />
          <Route path="archive" element={<ArchivePage />} />
          <Route path="settings" element={<GiftSettingsPage />} />
        </Route>
        <Route path="/status" element={<StatusPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
