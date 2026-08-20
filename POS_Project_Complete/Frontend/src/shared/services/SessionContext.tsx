import { createContext, useContext, useState, ReactNode } from 'react';
import { SessionUser } from './authTypes';
import { getToken } from './apiClient';

interface SessionContextValue {
  user: SessionUser | null;
  setUser: (user: SessionUser | null) => void;
  isAuthenticated: boolean;
}

const SessionContext = createContext<SessionContextValue | undefined>(undefined);

export function SessionProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<SessionUser | null>(null);

  return (
    <SessionContext.Provider value={{ user, setUser, isAuthenticated: !!user || !!getToken() }}>
      {children}
    </SessionContext.Provider>
  );
}

export function useSession() {
  const ctx = useContext(SessionContext);
  if (!ctx) throw new Error('useSession must be used within SessionProvider');
  return ctx;
}
