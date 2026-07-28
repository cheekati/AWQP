import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { authApi } from '../api/services';
import type { UserInfo } from '../types';

interface AuthContextValue {
  user: UserInfo | null;
  token: string | null;
  loading: boolean;
  login: (userName: string, password: string) => Promise<void>;
  logout: () => void;
  hasRole: (...roles: string[]) => boolean;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserInfo | null>(() => {
    const raw = localStorage.getItem('cms_user');
    return raw ? (JSON.parse(raw) as UserInfo) : null;
  });
  const [token, setToken] = useState<string | null>(() => localStorage.getItem('cms_token'));
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const boot = async () => {
      if (!token) {
        setLoading(false);
        return;
      }
      try {
        const res = await authApi.me();
        if (res.data.success && res.data.data) {
          setUser(res.data.data);
          localStorage.setItem('cms_user', JSON.stringify(res.data.data));
        } else {
          logout();
        }
      } catch {
        logout();
      } finally {
        setLoading(false);
      }
    };
    void boot();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const login = useCallback(async (userName: string, password: string) => {
    const res = await authApi.login(userName, password);
    if (!res.data.success || !res.data.data) {
      throw new Error(res.data.message || 'Login failed');
    }
    localStorage.setItem('cms_token', res.data.data.token);
    localStorage.setItem('cms_user', JSON.stringify(res.data.data.user));
    setToken(res.data.data.token);
    setUser(res.data.data.user);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('cms_token');
    localStorage.removeItem('cms_user');
    setToken(null);
    setUser(null);
  }, []);

  const hasRole = useCallback(
    (...roles: string[]) => !!user?.roles.some((r) => roles.includes(r)),
    [user],
  );

  const value = useMemo(
    () => ({ user, token, loading, login, logout, hasRole }),
    [user, token, loading, login, logout, hasRole],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
