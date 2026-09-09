export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  bestGuessCount: number | null;
  createdAtUtc: string;
}

export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  user: UserProfile;
}

export type GameStatus = 'Active' | 'Completed';

export interface GameSummary {
  id: string;
  status: GameStatus;
  guessCount: number;
  startedAtUtc: string;
  completedAtUtc: string | null;
}

export type GuessOutcome = 'Higher' | 'Lower' | 'Correct';

export interface GuessResult {
  gameId: string;
  outcome: GuessOutcome;
  guessCount: number;
  isCompleted: boolean;
  personalBest: number | null;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface Achievement {
  code: string;
  name: string;
  description: string;
  unlocked: boolean;
}

export interface RecentGame {
  id: string;
  guessCount: number;
  completedAtUtc: string;
}

export interface Performance {
  completedGames: number;
  bestScore: number | null;
  averageGuesses: number | null;
  recentGames: RecentGame[];
  achievements: Achievement[];
}
