import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import { dashboardApi } from '../api/services';
import type { DashboardStats } from '../types';
import { LoadingBlock, StatusBadge } from '../components/Ui';
import { useAuth } from '../auth/AuthContext';

export function DashboardPage() {
  const { user, hasRole } = useAuth();
  const navigate = useNavigate();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const res = await dashboardApi.stats();
        setStats(res.data.data || null);
      } finally {
        setLoading(false);
      }
    };
    void load();
  }, []);

  if (loading) return <LoadingBlock />;
  if (!stats) return <div className="empty-state">Unable to load dashboard.</div>;

  return (
    <div className="page">
      <section className="page-intro">
        <h2>Welcome, {user?.firstName}</h2>
        <p>Track engineering change requests across verification and COO approval stages.</p>
      </section>

      <section className="stat-grid">
        <article>
          <span>Total</span>
          <strong>{stats.totalRequests}</strong>
        </article>
        <article>
          <span>In Progress</span>
          <strong>{stats.inProgressCount}</strong>
        </article>
        <article>
          <span>Approved</span>
          <strong>{stats.approvedCount}</strong>
        </article>
        <article>
          <span>Rejected</span>
          <strong>{stats.rejectedCount}</strong>
        </article>
        <article className="accent">
          <span>Awaiting My Action</span>
          <strong>{stats.awaitingMyAction}</strong>
        </article>
      </section>

      <div className="split">
        <section className="panel">
          <h3>Monthly Trend</h3>
          <div className="chart-box">
            <ResponsiveContainer width="100%" height={260}>
              <BarChart data={stats.monthlyTrend}>
                <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.08)" />
                <XAxis dataKey="month" stroke="#9fb0c3" />
                <YAxis allowDecimals={false} stroke="#9fb0c3" />
                <Tooltip />
                <Bar dataKey="count" fill="#2f9e8f" radius={[6, 6, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </section>

        <section className="panel">
          <div className="panel-head">
            <h3>Recent Requests</h3>
            {hasRole('Requester') && <Link to="/requests/new">Create ER</Link>}
          </div>
          <ul className="recent-list">
            {stats.recentRequests.map((r) => (
              <li key={r.id} onClick={() => navigate(`/requests/${r.id}`)}>
                <div>
                  <strong>{r.erNumber}</strong>
                  <span>{r.title}</span>
                </div>
                <StatusBadge status={r.status} label={r.statusName} />
              </li>
            ))}
            {!stats.recentRequests.length && <li className="muted">No recent requests.</li>}
          </ul>
        </section>
      </div>
    </div>
  );
}
