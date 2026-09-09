import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { LoginPage } from './LoginPage';
import { renderWithProviders } from '../../test/renderWithProviders';
import { server } from '../../test/server';

describe('LoginPage', () => {
  it('renders the sign-in form', async () => {
    renderWithProviders(<LoginPage />);
    expect(await screen.findByRole('heading', { name: /welcome back/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
  });

  it('shows a friendly message when credentials are invalid', async () => {
    server.use(
      http.post('/api/auth/login', () =>
        HttpResponse.json({ errorCode: 'INVALID_CREDENTIALS', detail: 'bad' }, { status: 401 }),
      ),
    );
    const user = userEvent.setup();
    renderWithProviders(<LoginPage />);

    await user.type(screen.getByLabelText(/email/i), 'player@example.com');
    await user.type(screen.getByLabelText(/password/i), 'WrongPass1');
    await user.click(screen.getByRole('button', { name: /sign in/i }));

    await waitFor(() =>
      expect(screen.getByRole('alert')).toHaveTextContent(/email or password is incorrect/i),
    );
  });
});
