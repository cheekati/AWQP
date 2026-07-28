import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { erApi } from '../api/client';
import type { ErListItem } from '../types';
import StatusBadge from '../components/StatusBadge';

export default function IndexPage() {
  const [items, setItems] = useState<ErListItem[]>([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    erApi
      .list()
      .then(setItems)
      .catch(() => setError('Failed to load engineering requests.'))
      .finally(() => setLoading(false));
  }, []);

  return (
    <section className="panel">
      <div className="panel-head">
        <div>
          <h1>Engineering Requests</h1>
          <p className="lede">View and edit your submitted change requests. ER numbers stay fixed on edit.</p>
        </div>
        <Link className="btn primary" to="/ers/new">
          New ER
        </Link>
      </div>

      {loading && <p>Loading…</p>}
      {error && <p className="error">{error}</p>}

      {!loading && !error && (
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>ER No</th>
                <th>Title</th>
                <th>Division</th>
                <th>Department</th>
                <th>Status</th>
                <th>Outcome</th>
                <th>Valid (days)</th>
                <th>Submission</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {items.length === 0 && (
                <tr>
                  <td colSpan={9} className="muted">
                    No engineering requests yet.
                  </td>
                </tr>
              )}
              {items.map((item) => (
                <tr key={item.id}>
                  <td className="mono">{item.displayErNumber}</td>
                  <td>{item.title}</td>
                  <td>{item.division}</td>
                  <td>{item.department}</td>
                  <td>
                    <StatusBadge status={item.status} />
                  </td>
                  <td>{item.outcome === 'None' ? '—' : item.outcome}</td>
                  <td>{item.daysRemaining}</td>
                  <td>
                    {item.submissionNumber}/{2}
                  </td>
                  <td className="row-actions">
                    <Link to={`/ers/${item.id}`}>View</Link>
                    {['Draft', 'Rejected', 'ResubmitRequested'].includes(item.status) && (
                      <Link to={`/ers/${item.id}/edit`}>Edit</Link>
                    )}
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
