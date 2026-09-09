import type { ReactElement } from 'react';
import { render, type RenderOptions } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AuthProvider } from '../shared/auth/AuthProvider';

export function renderWithProviders(
  ui: ReactElement,
  { route = '/', ...options }: { route?: string } & RenderOptions = {},
) {
  return render(
    <MemoryRouter initialEntries={[route]}>
      <AuthProvider>{ui}</AuthProvider>
    </MemoryRouter>,
    options,
  );
}
