import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { erApi } from '../api/client';
import StatusBadge from '../components/StatusBadge';
import type { ErListItem } from '../types';

export default function CooQueuePage() {
  const [items, setItems] = useState<ErListItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    erApi
      .cooQueue()
      .then(setItems)
      .finally(() => setLoading(false));
  }, []);

  return (
    <section className="panel">
      <div className="panel-head">
        <div>
          <h1>COO Approval</h1>
          <p className="lede">
            ERs with Safety, Dept Head, and QA approval. Choose Approve, Reject, or Re-Submit.
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
                <th>Division</th>
                <th>Status</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {items.length === 0 && (
                <tr>
                  <td colSpan={6} className="muted">
                    No ERs awaiting COO decision.
                  </td>
                </tr>
              )}
              {items.map((item) => (
                <tr key={item.id}>
                  <td className="mono">{item.displayErNumber}</td>
                  <td>{item.title}</td>
                  <td>{item.requesterName}</td>
                  <td>{item.division}</td>
                  <td>
                    <StatusBadge status={item.status} />
                  </td>
                  <td>
                    <Link to={`/ers/${item.id}`}>Decide</Link>
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
