import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import AppLayout from './shared/components/layout/AppLayout';
import { SessionProvider } from './shared/services/SessionContext';
import DashboardPage from './pages/dashboard/DashboardPage';
import PosPage from './pages/pos/PosPage';
import PlaceholderPage from './pages/PlaceholderPage';

export default function App() {
  return (
    <SessionProvider>
      <BrowserRouter>
        <AppLayout>
          <Routes>
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/pos" element={<PosPage />} />
            <Route path="/products" element={<PlaceholderPage title="المنتجات" />} />
            <Route path="/customers" element={<PlaceholderPage title="العملاء" />} />
            <Route path="/purchases" element={<PlaceholderPage title="المشتريات" />} />
            <Route path="/inventory" element={<PlaceholderPage title="المخزون" />} />
            <Route path="/payments" element={<PlaceholderPage title="الديون والمدفوعات" />} />
            <Route path="/cash" element={<PlaceholderPage title="الصندوق" />} />
            <Route path="/reports" element={<PlaceholderPage title="التقارير" />} />
            <Route path="/settings" element={<PlaceholderPage title="الإعدادات" />} />
          </Routes>
        </AppLayout>
      </BrowserRouter>
    </SessionProvider>
  );
}
