import { http, HttpResponse } from 'msw';
import type { AuthResponse, GameSummary, GuessResult, Performance } from '../shared/api/types';

export const testUser = {
  id: '11111111-1111-1111-1111-111111111111',
  email: 'player@example.com',
  displayName: 'Player One',
  bestGuessCount: null as number | null,
  createdAtUtc: '2026-01-01T00:00:00Z',
};

export function authResponse(overrides: Partial<AuthResponse> = {}): AuthResponse {
  return {
    accessToken: 'test-access-token',
    accessTokenExpiresAtUtc: '2026-01-01T00:15:00Z',
    user: testUser,
    ...overrides,
  };
}

const activeGame: GameSummary = {
  id: '22222222-2222-2222-2222-222222222222',
  status: 'Active',
  guessCount: 0,
  startedAtUtc: '2026-01-01T00:00:00Z',
  completedAtUtc: null,
};

export const defaultHandlers = [
  http.post('/api/auth/register', () => HttpResponse.json(authResponse(), { status: 201 })),
  http.post('/api/auth/login', () => HttpResponse.json(authResponse())),
  http.post('/api/auth/refresh', () => new HttpResponse(null, { status: 401 })),
  http.post('/api/auth/logout', () => new HttpResponse(null, { status: 204 })),
  http.get('/api/users/me', () => HttpResponse.json(testUser)),
  http.get('/api/games/active', () => new HttpResponse(null, { status: 204 })),
  http.post('/api/games', () => HttpResponse.json(activeGame)),
  http.post('/api/games/:id/guesses', () =>
    HttpResponse.json<GuessResult>({
      gameId: activeGame.id,
      outcome: 'Higher',
      guessCount: 1,
      isCompleted: false,
      personalBest: null,
    }),
  ),
  http.get('/api/games', () =>
    HttpResponse.json({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 }),
  ),
  http.get('/api/stats/performance', () =>
    HttpResponse.json<Performance>({
      completedGames: 0,
      bestScore: null,
      averageGuesses: null,
      recentGames: [],
      achievements: [
        {
          code: 'FIRST_WIN',
          name: 'First Win',
          description: 'Win your first game.',
          unlocked: false,
        },
      ],
    }),
  ),
];
