import { Navigate, Route, Routes } from 'react-router-dom';
import { ComingSoonPage } from '../../features/home/ComingSoonPage';
import { HomePage } from '../../features/home/HomePage';
import { StatusPage } from '../../features/status/StatusPage';
import { AppLayout } from '../shell/AppLayout';

export function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route index element={<HomePage />} />
        <Route path="/expenses" element={<ComingSoonPage title="Расходы" />} />
        <Route path="/shopping" element={<ComingSoonPage title="Покупки" />} />
        <Route path="/todos" element={<ComingSoonPage title="Дела" />} />
        <Route path="/gifts" element={<ComingSoonPage title="Подарки" />} />
        <Route path="/status" element={<StatusPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
