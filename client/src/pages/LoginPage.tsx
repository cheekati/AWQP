import type { FormEvent, ReactNode } from 'react';
import { useState } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { ShieldCheck } from 'lucide-react';
import { useAuth } from '../auth/AuthContext';

export function LoginPage() {
  const { login, token, loading } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [userName, setUserName] = useState('requester');
  const [password, setPassword] = useState('Password@123');
  const [busy, setBusy] = useState(false);

  if (!loading && token) {
    const redirect = (location.state as { from?: string } | null)?.from || '/dashboard';
    return <Navigate to={redirect} replace />;
  }

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setBusy(true);
    try {
      await login(userName, password);
      toast.success('Welcome back');
      navigate('/dashboard');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Login failed');
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-hero" aria-hidden="true" />
      <form className="login-panel" onSubmit={onSubmit}>
        <div className="login-brand">
          <ShieldCheck size={36} />
          <div>
            <h1>ChangeOps</h1>
            <p>Enterprise Engineering Request Management</p>
          </div>
        </div>
        <label>
          Username
          <input value={userName} onChange={(e) => setUserName(e.target.value)} required autoComplete="username" />
        </label>
        <label>
          Password
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            autoComplete="current-password"
          />
        </label>
        <button className="primary" type="submit" disabled={busy}>
          {busy ? 'Signing in...' : 'Sign in'}
        </button>
        <div className="demo-users">
          <p>Demo users (password: Password@123)</p>
          <code>requester · safety · depthead · qa · coo · admin</code>
        </div>
      </form>
    </div>
  );
}

export function ProtectedRoute({ children, roles }: { children: ReactNode; roles?: string[] }) {
  const { token, loading, hasRole } = useAuth();
  const location = useLocation();

  if (loading) {
    return (
      <div className="loading-block">
        <div className="spinner" />
        <p>Loading session...</p>
      </div>
    );
  }
  if (!token) return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  if (roles && !hasRole(...roles, 'Administrator')) {
    return <div className="empty-state">You do not have access to this page.</div>;
  }
  return <>{children}</>;
}
