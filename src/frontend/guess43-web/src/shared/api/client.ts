import { tokenStore } from './tokenStore';
import type { AuthResponse } from './types';

export interface ProblemDetails {
  status: number;
  errorCode: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

/** Error thrown for non-success API responses, keyed off the stable server errorCode. */
export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly errorCode: string,
    message: string,
    public readonly fieldErrors?: Record<string, string[]>,
  ) {
    super(message);
    this.name = 'ApiError';
  }

  static async fromResponse(response: Response): Promise<ApiError> {
    let code = 'UNKNOWN';
    let detail = response.statusText;
    let fieldErrors: Record<string, string[]> | undefined;
    try {
      const problem = (await response.json()) as ProblemDetails & { title?: string };
      code = problem.errorCode ?? code;
      detail = problem.detail ?? problem.title ?? detail;
      fieldErrors = problem.errors;
    } catch {
      // Non-JSON error body; keep defaults.
    }
    return new ApiError(response.status, code, detail, fieldErrors);
  }
}

let onSessionExpired: (() => void) | null = null;
export function setOnSessionExpired(callback: (() => void) | null): void {
  onSessionExpired = callback;
}

let refreshPromise: Promise<boolean> | null = null;

/** Single-flight refresh to prevent stampedes when multiple requests fail at once. */
async function tryRefresh(): Promise<boolean> {
  refreshPromise ??= fetch('/api/auth/refresh', { method: 'POST', credentials: 'include' })
    .then(async (response) => {
      if (!response.ok) {
        return false;
      }
      const data = (await response.json()) as AuthResponse;
      tokenStore.set(data.accessToken);
      return true;
    })
    .catch(() => false)
    .finally(() => {
      refreshPromise = null;
    });
  return refreshPromise;
}

function buildHeaders(options: RequestInit): Headers {
  const headers = new Headers(options.headers);
  if (options.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }
  const token = tokenStore.get();
  if (token) {
    headers.set('Authorization', `Bearer ${token}`);
  }
  return headers;
}

async function parse<T>(response: Response): Promise<T> {
  if (response.status === 204) {
    return undefined as T;
  }
  return (await response.json()) as T;
}

/**
 * Performs an authenticated request. On a 401 it attempts a single refresh and
 * retries the original request exactly once. Refresh loops are prevented by the
 * `retry` guard and by never refreshing for /api/auth/* endpoints.
 */
export async function apiFetch<T>(
  path: string,
  options: RequestInit = {},
  retry = true,
): Promise<T> {
  const response = await fetch(path, {
    ...options,
    headers: buildHeaders(options),
    credentials: 'include',
  });

  if (response.status === 401 && retry && !path.startsWith('/api/auth/')) {
    const refreshed = await tryRefresh();
    if (refreshed) {
      return apiFetch<T>(path, options, false);
    }
    tokenStore.clear();
    onSessionExpired?.();
  }

  if (!response.ok) {
    throw await ApiError.fromResponse(response);
  }

  return parse<T>(response);
}
