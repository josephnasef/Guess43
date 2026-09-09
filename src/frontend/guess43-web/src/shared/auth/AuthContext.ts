import { createContext } from 'react';
import type { UserProfile } from '../api/types';

export type AuthStatus = 'loading' | 'authenticated' | 'anonymous';

export interface AuthContextValue {
  status: AuthStatus;
  user: UserProfile | null;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, displayName: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  setUser: (user: UserProfile) => void;
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);
