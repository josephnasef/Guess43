// In-memory access token store. The access token is intentionally NOT persisted
// to localStorage; it lives only in memory. The refresh token is an HttpOnly cookie.
let accessToken: string | null = null;

export const tokenStore = {
  get: (): string | null => accessToken,
  set: (token: string | null): void => {
    accessToken = token;
  },
  clear: (): void => {
    accessToken = null;
  },
};
