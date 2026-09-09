import '@testing-library/jest-dom/vitest';
import { afterAll, afterEach, beforeAll } from 'vitest';
import { server } from './test/server';
import { tokenStore } from './shared/api/tokenStore';

beforeAll(() => server.listen({ onUnhandledRequest: 'error' }));
afterEach(() => {
  server.resetHandlers();
  tokenStore.clear();
});
afterAll(() => server.close());
