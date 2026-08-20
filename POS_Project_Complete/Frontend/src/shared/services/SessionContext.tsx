import { createContext, useContext, useState, ReactNode } from 'react';
import { SessionUser } from './authTypes';

interface SessionContextValue {
  user: SessionUser | null;
  setUser: (user: SessionUser | null) => void;
}

const SessionContext = createContext<SessionContextValue | undefined>(undefined);

export function SessionProvider({ children }: { children: ReactNode }) {
  // بيانات تجريبية (Mock) مؤقتًا — تُستبدل لاحقًا بنتيجة POST /api/auth/login الحقيقية
  const [user, setUser] = useState<SessionUser | null>({
    userId: 'mock-user-1',
    username: 'admin',
    fullName: 'مدير النظام',
    roles: ['Administrator'],
    permissions: [
      'CanSell', 'CanDiscount', 'CanChangePrice', 'CanDeleteSale', 'CanReturn',
      'CanViewCost', 'CanViewProfit', 'CanModifyStock', 'CanCloseCash',
      'CanManageUsers', 'CanViewReports'
    ]
  });

  return (
    <SessionContext.Provider value={{ user, setUser }}>
      {children}
    </SessionContext.Provider>
  );
}

export function useSession() {
  const ctx = useContext(SessionContext);
  if (!ctx) throw new Error('useSession must be used within SessionProvider');
  return ctx;
}
