import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { usersApi } from './endpoints';
import { tokenStore } from './tokenStore';
import { ApiError } from './client';
import { server } from '../../test/server';
import { authResponse, testUser } from '../../test/handlers';

describe('api client refresh behaviour', () => {
  it('refreshes once on 401 and retries the original request', async () => {
    tokenStore.set('expired-token');
    let meCalls = 0;
    let refreshCalls = 0;
    server.use(
      http.get('/api/users/me', () => {
        meCalls += 1;
        if (meCalls === 1) {
          return new HttpResponse(null, { status: 401 });
        }
        return HttpResponse.json(testUser);
      }),
      http.post('/api/auth/refresh', () => {
        refreshCalls += 1;
        return HttpResponse.json(authResponse({ accessToken: 'fresh-token' }));
      }),
    );

    const profile = await usersApi.me();

    expect(profile.email).toBe(testUser.email);
    expect(refreshCalls).toBe(1);
    expect(meCalls).toBe(2);
    expect(tokenStore.get()).toBe('fresh-token');
  });

  it('surfaces an ApiError with the stable error code when refresh fails', async () => {
    tokenStore.set('expired-token');
    server.use(
      http.get('/api/users/me', () => new HttpResponse(null, { status: 401 })),
      http.post('/api/auth/refresh', () => new HttpResponse(null, { status: 401 })),
    );

    await expect(usersApi.me()).rejects.toBeInstanceOf(ApiError);
  });
});
