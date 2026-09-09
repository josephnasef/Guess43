import { describe, expect, it } from 'vitest';
import { Route, Routes } from 'react-router-dom';
import { screen, waitFor } from '@testing-library/react';
import { ProtectedRoute } from './RouteGuards';
import { renderWithProviders } from '../../test/renderWithProviders';

describe('ProtectedRoute', () => {
  it('redirects anonymous users to the login page', async () => {
    renderWithProviders(
      <Routes>
        <Route element={<ProtectedRoute />}>
          <Route path="/" element={<div>Secret dashboard</div>} />
        </Route>
        <Route path="/login" element={<div>Login screen</div>} />
      </Routes>,
    );

    // The bootstrap refresh returns 401 (default handler) so the user is anonymous.
    await waitFor(() => expect(screen.getByText('Login screen')).toBeInTheDocument());
    expect(screen.queryByText('Secret dashboard')).not.toBeInTheDocument();
  });
});
