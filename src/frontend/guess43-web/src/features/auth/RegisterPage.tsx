import { useMemo, useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { ApiError } from '../../shared/api/client';
import { useAuth } from '../../shared/auth/useAuth';
import { toFriendlyMessage } from '../../shared/errors';

interface FieldErrors {
  email?: string;
  displayName?: string;
  password?: string;
}

export function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [submitting, setSubmitting] = useState(false);

  const clientErrors = useMemo<FieldErrors>(() => {
    const errors: FieldErrors = {};
    if (password && password.length < 8) {
      errors.password = 'Use at least 8 characters.';
    }
    if (displayName && displayName.trim().length < 2) {
      errors.displayName = 'Display name is too short.';
    }
    return errors;
  }, [password, displayName]);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setError(null);
    setFieldErrors({});
    setSubmitting(true);
    try {
      await register(email, displayName, password);
      navigate('/', { replace: true });
    } catch (err) {
      if (err instanceof ApiError && err.fieldErrors) {
        const mapped: FieldErrors = {};
        for (const [key, value] of Object.entries(err.fieldErrors)) {
          const field = key.charAt(0).toLowerCase() + key.slice(1);
          if (field === 'email' || field === 'displayName' || field === 'password') {
            mapped[field] = value.join(' ');
          }
        }
        setFieldErrors(mapped);
      }
      setError(toFriendlyMessage(err));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="auth-card">
      <h1>Create your account</h1>
      <p className="auth-card__subtitle">Guess the secret number in as few tries as you can.</p>
      <form onSubmit={handleSubmit} noValidate>
        {error && (
          <div className="alert alert--error" role="alert">
            {error}
          </div>
        )}
        <label className="field">
          <span>Email</span>
          <input
            type="email"
            value={email}
            autoComplete="email"
            required
            onChange={(e) => setEmail(e.target.value)}
          />
          {fieldErrors.email && <small className="field__error">{fieldErrors.email}</small>}
        </label>
        <label className="field">
          <span>Display name</span>
          <input
            type="text"
            value={displayName}
            autoComplete="nickname"
            required
            onChange={(e) => setDisplayName(e.target.value)}
          />
          {(fieldErrors.displayName ?? clientErrors.displayName) && (
            <small className="field__error">
              {fieldErrors.displayName ?? clientErrors.displayName}
            </small>
          )}
        </label>
        <label className="field">
          <span>Password</span>
          <input
            type="password"
            value={password}
            autoComplete="new-password"
            required
            onChange={(e) => setPassword(e.target.value)}
          />
          {(fieldErrors.password ?? clientErrors.password) && (
            <small className="field__error">{fieldErrors.password ?? clientErrors.password}</small>
          )}
        </label>
        <button
          type="submit"
          className="btn btn--primary"
          disabled={submitting || Object.keys(clientErrors).length > 0}
        >
          {submitting ? 'Creating…' : 'Create account'}
        </button>
      </form>
      <p className="auth-card__switch">
        Already have an account? <Link to="/login">Sign in</Link>
      </p>
    </div>
  );
}
