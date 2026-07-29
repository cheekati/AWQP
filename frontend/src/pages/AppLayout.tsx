import { Link, NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function AppLayout() {
  const { user, logout } = useAuth();
  if (!user) return null;

  const showVerify = ['Safety', 'DeptHead', 'QA', 'Admin'].includes(user.role);
  const showCoo = ['COO', 'Admin'].includes(user.role);

  return (
    <div className="app-shell">
      <header className="topbar">
        <Link to="/" className="brand">
          <span className="brand-mark">CMS</span>
          <span>Change Management</span>
        </Link>
        <nav>
          <NavLink to="/" end>
            My ERs
          </NavLink>
          {(user.role === 'Requester' || user.role === 'Admin') && (
            <NavLink to="/ers/new">New ER</NavLink>
          )}
          {showVerify && <NavLink to="/verify">Verification</NavLink>}
          {showCoo && <NavLink to="/coo">COO Approval</NavLink>}
        </nav>
        <div className="user-chip">
          <div>
            <strong>{user.fullName}</strong>
            <span>
              {user.role}
              {user.department ? ` · ${user.department}` : ''}
            </span>
          </div>
          <button type="button" className="btn ghost" onClick={logout}>
            Sign out
          </button>
        </div>
      </header>
      <main className="page">
        <Outlet />
      </main>
    </div>
  );
}
