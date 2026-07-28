import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { erApi } from '../api/services';
import type { EngineeringRequestListItem, ErStatus } from '../types';
import { DataTable, LoadingBlock, Pagination, StatusBadge } from '../components/Ui';

export function RequestListPage({ mode = 'mine' }: { mode?: 'mine' | 'all' }) {
  const navigate = useNavigate();
  const [rows, setRows] = useState<EngineeringRequestListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const load = async () => {
    setLoading(true);
    try {
      const params = {
        page,
        pageSize: 10,
        search: search || undefined,
        status: status || undefined,
        sortBy: 'createdDate',
        sortDescending: true,
      };
      const res = mode === 'all' ? await erApi.all(params) : await erApi.mine(params);
      setRows(res.data.data?.items || []);
      setTotalPages(res.data.data?.totalPages || 1);
    } catch {
      toast.error('Failed to load requests');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, mode]);

  return (
    <div className="page">
      <section className="page-intro row">
        <div>
          <h2>{mode === 'all' ? 'All Engineering Requests' : 'My Engineering Requests'}</h2>
          <p>Search, filter, and open requests. ER numbers never change after creation.</p>
        </div>
        {mode === 'mine' && (
          <Link className="primary" to="/requests/new">
            New Engineering Request
          </Link>
        )}
      </section>

      <section className="filters">
        <input
          placeholder="Search ER number, title, customer"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <select value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">All statuses</option>
          {(
            [
              'Draft',
              'Submitted',
              'InProgress',
              'AwaitingCooApproval',
              'Approved',
              'Rejected',
              'Resubmitted',
              'Closed',
            ] as ErStatus[]
          ).map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </select>
        <button
          type="button"
          className="primary"
          onClick={() => {
            setPage(1);
            void load();
          }}
        >
          Apply
        </button>
      </section>

      {loading ? (
        <LoadingBlock />
      ) : (
        <>
          <DataTable
            rows={rows}
            onRowClick={(r) => navigate(`/requests/${r.id}`)}
            columns={[
              { key: 'er', header: 'ER Number', render: (r) => <strong>{r.erNumber}</strong> },
              { key: 'title', header: 'Title', render: (r) => r.title },
              { key: 'dept', header: 'Department', render: (r) => r.departmentName },
              { key: 'product', header: 'Product', render: (r) => r.productName },
              {
                key: 'status',
                header: 'Status',
                render: (r) => <StatusBadge status={r.status} label={r.statusName} />,
              },
              {
                key: 'actions',
                header: 'Actions',
                render: (r) => (
                  <div className="row-actions" onClick={(e) => e.stopPropagation()}>
                    <button type="button" onClick={() => navigate(`/requests/${r.id}`)}>
                      View
                    </button>
                    {['Draft', 'Rejected', 'Resubmitted'].includes(r.status) && (
                      <button type="button" onClick={() => navigate(`/requests/${r.id}/edit`)}>
                        Edit
                      </button>
                    )}
                  </div>
                ),
              },
            ]}
          />
          <Pagination page={page} totalPages={totalPages} onChange={setPage} />
        </>
      )}
    </div>
  );
}
