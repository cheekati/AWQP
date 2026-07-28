import { useState, type FormEvent } from 'react';
import { Navigate, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const DEMO_ACCOUNTS = [
  { email: 'requester@cms.local', role: 'Requester' },
  { email: 'safety@cms.local', role: 'Safety' },
  { email: 'depthead@cms.local', role: 'Dept Head' },
  { email: 'qa@cms.local', role: 'QA' },
  { email: 'coo@cms.local', role: 'COO' },
];

export default function LoginPage() {
  const { user, login, loading } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('requester@cms.local');
  const [password, setPassword] = useState('Password123!');
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  if (!loading && user) return <Navigate to="/" replace />;

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    setSubmitting(true);
    try {
      await login(email, password);
      navigate('/');
    } catch {
      setError('Invalid email or password.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="login-shell">
      <div className="login-panel">
        <p className="brand-mark">CMS</p>
        <h1>Change Management System</h1>
        <p className="lede">Engineering Request portal for controlled process and material changes.</p>

        <form className="stack" onSubmit={onSubmit}>
          <label>
            Email
            <input value={email} onChange={(e) => setEmail(e.target.value)} type="email" required />
          </label>
          <label>
            Password
            <input
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              type="password"
              required
            />
          </label>
          {error && <p className="error">{error}</p>}
          <button className="btn primary" type="submit" disabled={submitting}>
            {submitting ? 'Signing in…' : 'Sign in'}
          </button>
        </form>

        <div className="demo-accounts">
          <p>Demo accounts (password: Password123!)</p>
          <ul>
            {DEMO_ACCOUNTS.map((a) => (
              <li key={a.email}>
                <button
                  type="button"
                  className="linkish"
                  onClick={() => {
                    setEmail(a.email);
                    setPassword('Password123!');
                  }}
                >
                  {a.role}
                </button>
                <span>{a.email}</span>
              </li>
            ))}
          </ul>
        </div>
      </div>
      <div className="login-visual" aria-hidden="true">
        <div className="orb" />
        <p>Engineering Request · Verification · COO Approval</p>
      </div>
    </div>
  );
}
