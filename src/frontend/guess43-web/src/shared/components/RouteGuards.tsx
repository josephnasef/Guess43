import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../auth/useAuth';
import { Spinner } from './Spinner';

export function ProtectedRoute() {
  const { status } = useAuth();
  if (status === 'loading') {
    return <Spinner label="Restoring your session" />;
  }
  return status === 'authenticated' ? <Outlet /> : <Navigate to="/login" replace />;
}

export function GuestRoute() {
  const { status } = useAuth();
  if (status === 'loading') {
    return <Spinner label="Loading" />;
  }
  return status === 'anonymous' ? <Outlet /> : <Navigate to="/" replace />;
}
