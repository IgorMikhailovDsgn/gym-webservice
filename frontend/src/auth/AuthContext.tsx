import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
import { authApi } from '../api/endpoints';
import { tokenStorage } from '../api/client';
import type { AuthenticatedUser } from '../api/types';

const USER_KEY = 'gym_user';

interface AuthContextValue {
  user: AuthenticatedUser | null;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function readStoredUser(): AuthenticatedUser | null {
  const raw = localStorage.getItem(USER_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as AuthenticatedUser;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthenticatedUser | null>(readStoredUser);

  const value = useMemo<AuthContextValue>(() => ({
    user,
    login: async (username, password) => {
      const authenticated = await authApi.login(username, password);
      tokenStorage.set(authenticated.token);
      localStorage.setItem(USER_KEY, JSON.stringify(authenticated));
      setUser(authenticated);
    },
    logout: () => {
      tokenStorage.clear();
      localStorage.removeItem(USER_KEY);
      setUser(null);
    },
  }), [user]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth должен вызываться внутри AuthProvider');
  return context;
}
