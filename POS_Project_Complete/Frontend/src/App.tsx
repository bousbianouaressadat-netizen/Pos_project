import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import AppLayout from './shared/components/layout/AppLayout';
import { SessionProvider, useSession } from './shared/services/SessionContext';
import LoginPage from './pages/auth/LoginPage';
import DashboardPage from './pages/dashboard/DashboardPage';
import PosPage from './pages/pos/PosPage';
import PlaceholderPage from './pages/PlaceholderPage';

function ProtectedApp() {
  const { isAuthenticated } = useSession();

  if (!isAuthenticated) {
    return (
      <Routes>
        <Route path="*" element={<LoginPage />} />
      </Routes>
    );
  }

  return (
    <AppLayout>
      <Routes>
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="/login" element={<Navigate to="/dashboard" replace />} />
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
  );
}

export default function App() {
  return (
    <SessionProvider>
      <BrowserRouter>
        <ProtectedApp />
      </BrowserRouter>
    </SessionProvider>
  );
}
