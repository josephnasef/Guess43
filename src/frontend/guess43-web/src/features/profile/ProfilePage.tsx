import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { usersApi } from '../../shared/api/endpoints';
import { useAuth } from '../../shared/auth/useAuth';
import { toFriendlyMessage } from '../../shared/errors';

export function ProfilePage() {
  const { user, setUser, logout } = useAuth();
  const navigate = useNavigate();
  const [displayName, setDisplayName] = useState(user?.displayName ?? '');
  const [savedMessage, setSavedMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [confirmingDelete, setConfirmingDelete] = useState(false);

  const handleSave = async (event: FormEvent) => {
    event.preventDefault();
    setError(null);
    setSavedMessage(null);
    setSaving(true);
    try {
      const updated = await usersApi.update(displayName);
      setUser(updated);
      setSavedMessage('Profile updated.');
    } catch (err) {
      setError(toFriendlyMessage(err));
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    setError(null);
    try {
      await usersApi.remove();
      await logout();
      navigate('/register', { replace: true });
    } catch (err) {
      setError(toFriendlyMessage(err));
    }
  };

  return (
    <section className="profile">
      <h1>Your profile</h1>

      <div className="card">
        <form onSubmit={handleSave}>
          {savedMessage && (
            <div className="alert alert--success" role="status">
              {savedMessage}
            </div>
          )}
          {error && (
            <div className="alert alert--error" role="alert">
              {error}
            </div>
          )}
          <label className="field">
            <span>Email</span>
            <input type="email" value={user?.email ?? ''} disabled />
          </label>
          <label className="field">
            <span>Display name</span>
            <input
              type="text"
              value={displayName}
              required
              minLength={2}
              maxLength={50}
              onChange={(e) => setDisplayName(e.target.value)}
            />
          </label>
          <button type="submit" className="btn btn--primary" disabled={saving}>
            {saving ? 'Saving…' : 'Save changes'}
          </button>
        </form>
      </div>

      <div className="card card--danger">
        <h2>Danger zone</h2>
        <p className="muted">Deleting your account removes your games. This cannot be undone.</p>
        {confirmingDelete ? (
          <div className="confirm">
            <span>Are you sure?</span>
            <button type="button" className="btn btn--danger" onClick={handleDelete}>
              Delete my account
            </button>
            <button
              type="button"
              className="btn btn--ghost"
              onClick={() => setConfirmingDelete(false)}
            >
              Cancel
            </button>
          </div>
        ) : (
          <button
            type="button"
            className="btn btn--danger"
            onClick={() => setConfirmingDelete(true)}
          >
            Delete account
          </button>
        )}
      </div>
    </section>
  );
}
