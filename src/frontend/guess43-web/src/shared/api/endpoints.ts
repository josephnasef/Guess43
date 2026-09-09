import { apiFetch } from './client';
import { tokenStore } from './tokenStore';
import type {
  AuthResponse,
  GameSummary,
  GuessResult,
  PagedResult,
  Performance,
  UserProfile,
} from './types';

export const authApi = {
  register: (email: string, displayName: string, password: string) =>
    apiFetch<AuthResponse>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify({ email, displayName, password }),
    }),

  login: (email: string, password: string) =>
    apiFetch<AuthResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),

  logout: () => apiFetch<void>('/api/auth/logout', { method: 'POST' }),

  refresh: () => apiFetch<AuthResponse>('/api/auth/refresh', { method: 'POST' }, false),
};

export const usersApi = {
  me: () => apiFetch<UserProfile>('/api/users/me'),
  update: (displayName: string) =>
    apiFetch<UserProfile>('/api/users/me', {
      method: 'PUT',
      body: JSON.stringify({ displayName }),
    }),
  remove: () => apiFetch<void>('/api/users/me', { method: 'DELETE' }),
};

export const gamesApi = {
  start: () => apiFetch<GameSummary>('/api/games', { method: 'POST' }),
  active: async (): Promise<GameSummary | null> => {
    // 204 -> no active game.
    const result = await apiFetch<GameSummary | undefined>('/api/games/active');
    return result ?? null;
  },
  guess: (gameId: string, guess: number) =>
    apiFetch<GuessResult>(`/api/games/${gameId}/guesses`, {
      method: 'POST',
      body: JSON.stringify({ guess }),
    }),
  history: (page: number, pageSize: number) =>
    apiFetch<PagedResult<GameSummary>>(`/api/games?page=${page}&pageSize=${pageSize}`),
  get: (gameId: string) => apiFetch<GameSummary>(`/api/games/${gameId}`),
  remove: (gameId: string) => apiFetch<void>(`/api/games/${gameId}`, { method: 'DELETE' }),
};

export const statsApi = {
  performance: () => apiFetch<Performance>('/api/stats/performance'),
};

export { tokenStore };
