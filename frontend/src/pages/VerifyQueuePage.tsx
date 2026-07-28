import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { erApi } from '../api/client';
import StatusBadge from '../components/StatusBadge';
import { useAuth } from '../context/AuthContext';
import type { ErListItem } from '../types';

export default function VerifyQueuePage() {
  const { user } = useAuth();
  const [items, setItems] = useState<ErListItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    erApi
      .verificationQueue()
      .then(setItems)
      .finally(() => setLoading(false));
  }, []);

  return (
    <section className="panel">
      <div className="panel-head">
        <div>
          <h1>Verification Queue</h1>
          <p className="lede">
            Default landing for {user?.role} — approve or reject assigned ERs without modifying content.
          </p>
        </div>
      </div>
      {loading ? (
        <p>Loading…</p>
      ) : (
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>ER No</th>
                <th>Title</th>
                <th>Requester</th>
                <th>Department</th>
                <th>Status</th>
                <th>Submitted</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {items.length === 0 && (
                <tr>
                  <td colSpan={7} className="muted">
                    No pending verifications.
                  </td>
                </tr>
              )}
              {items.map((item) => (
                <tr key={item.id}>
                  <td className="mono">{item.displayErNumber}</td>
                  <td>{item.title}</td>
                  <td>{item.requesterName}</td>
                  <td>{item.department}</td>
                  <td>
                    <StatusBadge status={item.status} />
                  </td>
                  <td>{item.submittedAt ? new Date(item.submittedAt).toLocaleString() : '—'}</td>
                  <td>
                    <Link to={`/ers/${item.id}`}>Review</Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
