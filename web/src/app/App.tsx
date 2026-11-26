import { Navigate, Route, Routes } from 'react-router-dom';
import { StatusPage } from '../features/status/StatusPage';
import { AppLayout } from './AppLayout';

export function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route index element={<Navigate to="/status" replace />} />
        <Route path="/status" element={<StatusPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
