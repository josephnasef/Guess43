import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/useAuth';

export function AppLayout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login', { replace: true });
  };

  return (
    <div className="app-shell">
      <header className="app-header">
        <div className="app-header__brand">
          <span className="app-header__logo" aria-hidden="true">
            43
          </span>
          <span>Guess43</span>
        </div>
        <nav className="app-nav" aria-label="Primary">
          <NavLink to="/" end>
            Dashboard
          </NavLink>
          <NavLink to="/play">Play</NavLink>
          <NavLink to="/history">History</NavLink>
          <NavLink to="/profile">Profile</NavLink>
        </nav>
        <div className="app-header__user">
          {user && <span className="app-header__name">{user.displayName}</span>}
          <button type="button" className="btn btn--ghost" onClick={handleLogout}>
            Log out
          </button>
        </div>
      </header>
      <main className="app-main">
        <Outlet />
      </main>
    </div>
  );
}
