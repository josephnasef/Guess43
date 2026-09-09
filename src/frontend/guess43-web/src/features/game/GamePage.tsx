import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { gamesApi } from '../../shared/api/endpoints';
import type { GuessOutcome } from '../../shared/api/types';
import { useAuth } from '../../shared/auth/useAuth';
import { Spinner } from '../../shared/components/Spinner';
import { toFriendlyMessage } from '../../shared/errors';
import { Confetti } from './Confetti';

interface Attempt {
  value: number;
  outcome: GuessOutcome;
}

export function GamePage() {
  const { user, setUser } = useAuth();
  const [loading, setLoading] = useState(true);
  const [gameId, setGameId] = useState<string | null>(null);
  const [attempts, setAttempts] = useState<Attempt[]>([]);
  const [range, setRange] = useState({ low: 1, high: 43 });
  const [guess, setGuess] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [won, setWon] = useState(false);
  const [newRecord, setNewRecord] = useState(false);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const active = await gamesApi.active();
        if (cancelled) return;
        if (active) {
          setGameId(active.id);
          setAttempts([]);
        }
      } catch (err) {
        if (!cancelled) setError(toFriendlyMessage(err));
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const startGame = useCallback(async () => {
    setError(null);
    setSubmitting(true);
    try {
      const game = await gamesApi.start();
      setGameId(game.id);
      setAttempts([]);
      setRange({ low: 1, high: 43 });
      setGuess('');
      setWon(false);
      setNewRecord(false);
    } catch (err) {
      setError(toFriendlyMessage(err));
    } finally {
      setSubmitting(false);
    }
  }, []);

  const submitGuess = async (event: FormEvent) => {
    event.preventDefault();
    if (!gameId) return;
    const value = Number(guess);
    if (!Number.isInteger(value) || value < 1 || value > 43) {
      setError('Enter a whole number between 1 and 43.');
      return;
    }
    setError(null);
    setSubmitting(true);
    try {
      const result = await gamesApi.guess(gameId, value);
      setAttempts((prev) => [...prev, { value, outcome: result.outcome }]);
      setGuess('');
      if (result.outcome === 'Higher') {
        setRange((r) => ({ ...r, low: Math.max(r.low, value + 1) }));
      } else if (result.outcome === 'Lower') {
        setRange((r) => ({ ...r, high: Math.min(r.high, value - 1) }));
      } else {
        setWon(true);
        setGameId(null);
        if (user) {
          const previousBest = user.bestGuessCount;
          if (result.personalBest !== null) {
            setNewRecord(previousBest === null || result.personalBest < previousBest);
            setUser({ ...user, bestGuessCount: result.personalBest });
          }
        }
      }
    } catch (err) {
      setError(toFriendlyMessage(err));
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return <Spinner label="Loading your game" />;
  }

  return (
    <section className="game">
      <Confetti active={won} />
      <div className="game__panel card">
        <h1>Guess the Number</h1>
        <p className="game__hint">
          I'm thinking of a number between <strong>{range.low}</strong> and{' '}
          <strong>{range.high}</strong>.
        </p>

        {won && (
          <div className="alert alert--success" role="status">
            🎉 Correct in <strong>{attempts.length}</strong>{' '}
            {attempts.length === 1 ? 'guess' : 'guesses'}!
            {newRecord && <span> New personal best!</span>}
          </div>
        )}

        {error && (
          <div className="alert alert--error" role="alert">
            {error}
          </div>
        )}

        {gameId ? (
          <form onSubmit={submitGuess} className="game__form">
            <label className="field">
              <span>Your guess</span>
              <input
                type="number"
                min={1}
                max={43}
                inputMode="numeric"
                value={guess}
                autoFocus
                required
                onChange={(e) => setGuess(e.target.value)}
              />
            </label>
            <button type="submit" className="btn btn--primary" disabled={submitting}>
              {submitting ? 'Checking…' : 'Guess'}
            </button>
          </form>
        ) : (
          <button
            type="button"
            className="btn btn--primary"
            onClick={startGame}
            disabled={submitting}
          >
            {won ? 'Play again' : 'Start a new game'}
          </button>
        )}

        <p className="game__best">
          Personal best: <strong>{user?.bestGuessCount ?? '—'}</strong>
        </p>
      </div>

      <div className="game__attempts card" aria-live="polite">
        <h2>Your guesses</h2>
        {attempts.length === 0 ? (
          <p className="muted">No guesses yet. Make your first move!</p>
        ) : (
          <ol className="attempts">
            {attempts.map((attempt, index) => (
              <li
                key={index}
                className={`attempts__item attempts__item--${attempt.outcome.toLowerCase()}`}
              >
                <span>{attempt.value}</span>
                <span>
                  {attempt.outcome === 'Correct'
                    ? 'Correct!'
                    : `Go ${attempt.outcome.toLowerCase()}`}
                </span>
              </li>
            ))}
          </ol>
        )}
      </div>
    </section>
  );
}
