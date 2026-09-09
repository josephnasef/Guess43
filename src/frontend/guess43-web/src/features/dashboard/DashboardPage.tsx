import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { statsApi } from '../../shared/api/endpoints';
import type { Performance } from '../../shared/api/types';
import { useAuth } from '../../shared/auth/useAuth';
import { Spinner } from '../../shared/components/Spinner';
import { toFriendlyMessage } from '../../shared/errors';

export function DashboardPage() {
  const { user } = useAuth();
  const [performance, setPerformance] = useState<Performance | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const data = await statsApi.performance();
        if (!cancelled) setPerformance(data);
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

  return (
    <section className="dashboard">
      <div className="dashboard__hero card">
        <div>
          <p className="muted">Welcome back,</p>
          <h1>{user?.displayName}</h1>
        </div>
        <Link to="/play" className="btn btn--primary btn--lg">
          Play now
        </Link>
      </div>

      {loading && <Spinner label="Loading your stats" />}
      {error && (
        <div className="alert alert--error" role="alert">
          {error}
        </div>
      )}

      {performance && (
        <>
          <div className="stat-grid">
            <StatCard label="Personal best" value={user?.bestGuessCount ?? '—'} />
            <StatCard label="Games won" value={performance.completedGames} />
            <StatCard
              label="Avg. guesses"
              value={
                performance.averageGuesses !== null ? performance.averageGuesses.toFixed(1) : '—'
              }
            />
          </div>

          <div className="card">
            <h2>Achievements</h2>
            <ul className="achievements">
              {performance.achievements.map((a) => (
                <li
                  key={a.code}
                  className={`achievements__item ${a.unlocked ? 'is-unlocked' : 'is-locked'}`}
                >
                  <span className="achievements__icon" aria-hidden="true">
                    {a.unlocked ? '★' : '☆'}
                  </span>
                  <div>
                    <strong>{a.name}</strong>
                    <p className="muted">{a.description}</p>
                  </div>
                </li>
              ))}
            </ul>
          </div>

          <div className="card">
            <h2>Recent games</h2>
            {performance.recentGames.length === 0 ? (
              <p className="muted">You haven't finished a game yet. Your first win is waiting!</p>
            ) : (
              <ul className="recent">
                {performance.recentGames.map((g) => (
                  <li key={g.id}>
                    <span>{new Date(g.completedAtUtc).toLocaleDateString()}</span>
                    <span>
                      {g.guessCount} {g.guessCount === 1 ? 'guess' : 'guesses'}
                    </span>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </>
      )}
    </section>
  );
}

function StatCard({ label, value }: { label: string; value: string | number }) {
  return (
    <div className="stat-card card">
      <span className="stat-card__value">{value}</span>
      <span className="stat-card__label">{label}</span>
    </div>
  );
}
