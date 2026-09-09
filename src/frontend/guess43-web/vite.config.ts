import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

// The dev server proxies /api to the backend so the browser treats the SPA and
// API as same-origin, keeping the HttpOnly refresh cookie first-party.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5063',
        changeOrigin: true,
      },
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/setupTests.ts'],
    css: false,
    exclude: ['e2e/**', 'node_modules/**'],
  },
});
