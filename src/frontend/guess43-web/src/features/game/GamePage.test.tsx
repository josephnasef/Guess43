import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { GamePage } from './GamePage';
import { renderWithProviders } from '../../test/renderWithProviders';
import { server } from '../../test/server';
import { tokenStore } from '../../shared/api/tokenStore';

describe('GamePage', () => {
  it('shows higher/lower feedback and a win state', async () => {
    tokenStore.set('test-token');
    let guessCount = 0;
    server.use(
      http.post('/api/games/:id/guesses', () => {
        guessCount += 1;
        if (guessCount === 1) {
          return HttpResponse.json({
            gameId: '22222222-2222-2222-2222-222222222222',
            outcome: 'Higher',
            guessCount: 1,
            isCompleted: false,
            personalBest: null,
          });
        }
        return HttpResponse.json({
          gameId: '22222222-2222-2222-2222-222222222222',
          outcome: 'Correct',
          guessCount: 2,
          isCompleted: true,
          personalBest: 2,
        });
      }),
    );

    const user = userEvent.setup();
    renderWithProviders(<GamePage />);

    await user.click(await screen.findByRole('button', { name: /start a new game/i }));

    const input = await screen.findByLabelText(/your guess/i);
    await user.type(input, '10');
    await user.click(screen.getByRole('button', { name: /^guess$/i }));
    await waitFor(() => expect(screen.getByText(/go higher/i)).toBeInTheDocument());

    await user.type(await screen.findByLabelText(/your guess/i), '30');
    await user.click(screen.getByRole('button', { name: /^guess$/i }));

    await waitFor(() => expect(screen.getByRole('status')).toHaveTextContent(/correct in/i));
    expect(screen.getByRole('button', { name: /play again/i })).toBeInTheDocument();
  });
});
