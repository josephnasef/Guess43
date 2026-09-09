import { useCallback, useEffect, useState } from 'react';
import { gamesApi } from '../../shared/api/endpoints';
import type { GameSummary } from '../../shared/api/types';
import { Spinner } from '../../shared/components/Spinner';
import { toFriendlyMessage } from '../../shared/errors';

const PAGE_SIZE = 10;

export function HistoryPage() {
  const [page, setPage] = useState(1);
  const [data, setData] = useState<{ items: GameSummary[]; totalPages: number } | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pendingDelete, setPendingDelete] = useState<string | null>(null);

  const load = useCallback(async (targetPage: number) => {
    setLoading(true);
    setError(null);
    try {
      const result = await gamesApi.history(targetPage, PAGE_SIZE);
      setData({ items: result.items, totalPages: result.totalPages });
    } catch (err) {
      setError(toFriendlyMessage(err));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load(page);
  }, [page, load]);

  const handleDelete = async (id: string) => {
    setPendingDelete(null);
    try {
      await gamesApi.remove(id);
      await load(page);
    } catch (err) {
      setError(toFriendlyMessage(err));
    }
  };

  return (
    <section className="history">
      <h1>Game history</h1>

      {error && (
        <div className="alert alert--error" role="alert">
          {error}
        </div>
      )}

      {loading ? (
        <Spinner label="Loading history" />
      ) : !data || data.items.length === 0 ? (
        <p className="muted">No games yet. Head to Play to start your first one.</p>
      ) : (
        <>
          <table className="table">
            <thead>
              <tr>
                <th scope="col">Started</th>
                <th scope="col">Status</th>
                <th scope="col">Guesses</th>
                <th scope="col" aria-label="Actions" />
              </tr>
            </thead>
            <tbody>
              {data.items.map((game) => (
                <tr key={game.id}>
                  <td>{new Date(game.startedAtUtc).toLocaleString()}</td>
                  <td>
                    <span className={`badge badge--${game.status.toLowerCase()}`}>
                      {game.status}
                    </span>
                  </td>
                  <td>{game.guessCount}</td>
                  <td className="table__actions">
                    {game.status === 'Completed' &&
                      (pendingDelete === game.id ? (
                        <span className="confirm">
                          Delete?
                          <button
                            type="button"
                            className="btn btn--danger btn--sm"
                            onClick={() => handleDelete(game.id)}
                          >
                            Yes
                          </button>
                          <button
                            type="button"
                            className="btn btn--ghost btn--sm"
                            onClick={() => setPendingDelete(null)}
                          >
                            No
                          </button>
                        </span>
                      ) : (
                        <button
                          type="button"
                          className="btn btn--ghost btn--sm"
                          onClick={() => setPendingDelete(game.id)}
                        >
                          Delete
                        </button>
                      ))}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <div className="pagination">
            <button
              type="button"
              className="btn btn--ghost"
              disabled={page <= 1}
              onClick={() => setPage((p) => p - 1)}
            >
              Previous
            </button>
            <span>
              Page {page} of {Math.max(1, data.totalPages)}
            </span>
            <button
              type="button"
              className="btn btn--ghost"
              disabled={page >= data.totalPages}
              onClick={() => setPage((p) => p + 1)}
            >
              Next
            </button>
          </div>
        </>
      )}
    </section>
  );
}
