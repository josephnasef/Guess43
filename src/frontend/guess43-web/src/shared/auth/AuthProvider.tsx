import { useCallback, useEffect, useMemo, useState } from 'react';
import { setOnSessionExpired } from '../api/client';
import { authApi } from '../api/endpoints';
import { tokenStore } from '../api/tokenStore';
import type { UserProfile } from '../api/types';
import { AuthContext, type AuthStatus } from './AuthContext';

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [status, setStatus] = useState<AuthStatus>('loading');
  const [user, setUserState] = useState<UserProfile | null>(null);

  const clearSession = useCallback(() => {
    tokenStore.clear();
    setUserState(null);
    setStatus('anonymous');
  }, []);

  // Restore a session on load by attempting a silent refresh.
  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const auth = await authApi.refresh();
        if (cancelled) return;
        tokenStore.set(auth.accessToken);
        setUserState(auth.user);
        setStatus('authenticated');
      } catch {
        if (!cancelled) clearSession();
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [clearSession]);

  // When a background refresh fails, drop to the anonymous state.
  useEffect(() => {
    setOnSessionExpired(() => clearSession());
    return () => setOnSessionExpired(null);
  }, [clearSession]);

  const login = useCallback(async (email: string, password: string) => {
    const auth = await authApi.login(email, password);
    tokenStore.set(auth.accessToken);
    setUserState(auth.user);
    setStatus('authenticated');
  }, []);

  const register = useCallback(async (email: string, displayName: string, password: string) => {
    const auth = await authApi.register(email, displayName, password);
    tokenStore.set(auth.accessToken);
    setUserState(auth.user);
    setStatus('authenticated');
  }, []);

  const logout = useCallback(async () => {
    try {
      await authApi.logout();
    } finally {
      clearSession();
    }
  }, [clearSession]);

  const setUser = useCallback((next: UserProfile) => setUserState(next), []);

  const value = useMemo(
    () => ({ status, user, login, register, logout, setUser }),
    [status, user, login, register, logout, setUser],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
