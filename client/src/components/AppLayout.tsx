import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom';
import { Bell, ClipboardList, LayoutDashboard, LogOut, ShieldCheck } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useAuth } from '../auth/AuthContext';
import { notificationApi } from '../api/services';
import './layout.css';

export function AppLayout() {
  const { user, logout, hasRole } = useAuth();
  const navigate = useNavigate();
  const [unread, setUnread] = useState(0);

  useEffect(() => {
    const load = async () => {
      try {
        const res = await notificationApi.unreadCount();
        setUnread(res.data.data ?? 0);
      } catch {
        /* ignore */
      }
    };
    void load();
    const id = setInterval(load, 30000);
    return () => clearInterval(id);
  }, []);

  const onLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="shell">
      <aside className="sidebar">
        <div className="brand">
          <ShieldCheck size={28} />
          <div>
            <strong>ChangeOps</strong>
            <span>Engineering Requests</span>
          </div>
        </div>
        <nav>
          <NavLink to="/dashboard">
            <LayoutDashboard size={18} /> Dashboard
          </NavLink>
          {hasRole('Requester', 'Administrator') && (
            <>
              <NavLink to="/requests">
                <ClipboardList size={18} /> My Requests
              </NavLink>
              <NavLink to="/requests/new">New ER</NavLink>
            </>
          )}
          {hasRole('Safety', 'Administrator') && <NavLink to="/verify/safety">Safety Queue</NavLink>}
          {hasRole('DepartmentHead', 'Administrator') && (
            <NavLink to="/verify/department-head">Dept Head Queue</NavLink>
          )}
          {hasRole('QA', 'Administrator') && <NavLink to="/verify/qa">QA Queue</NavLink>}
          {hasRole('COO', 'Administrator') && <NavLink to="/coo">COO Approval</NavLink>}
          {hasRole('Administrator') && <NavLink to="/admin/requests">All Requests</NavLink>}
          <NavLink to="/notifications">
            <Bell size={18} /> Notifications {unread > 0 && <em className="badge">{unread}</em>}
          </NavLink>
        </nav>
        <div className="sidebar-footer">
          <div className="user-chip">
            <strong>{user?.fullName}</strong>
            <span>{user?.roles.join(', ')}</span>
          </div>
          <button type="button" className="ghost" onClick={onLogout}>
            <LogOut size={16} /> Sign out
          </button>
        </div>
      </aside>
      <main className="content">
        <header className="topbar">
          <div>
            <p className="eyebrow">Change Management System</p>
            <h1>Engineering Request Workflow</h1>
          </div>
          <Link to="/notifications" className="icon-btn" aria-label="Notifications">
            <Bell size={18} />
            {unread > 0 && <span>{unread}</span>}
          </Link>
        </header>
        <Outlet />
      </main>
    </div>
  );
}
