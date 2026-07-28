import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { cooApi, notificationApi, verificationApi } from '../api/services';
import type { EngineeringRequestListItem, NotificationItem } from '../types';
import { DataTable, LoadingBlock, Pagination, StatusBadge } from '../components/Ui';

export function VerificationQueuePage({
  stage,
  title,
}: {
  stage: 'safety' | 'department-head' | 'qa' | 'coo';
  title: string;
}) {
  const navigate = useNavigate();
  const [rows, setRows] = useState<EngineeringRequestListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [search, setSearch] = useState('');

  const load = async () => {
    setLoading(true);
    try {
      const params = { page, pageSize: 10, search: search || undefined };
      const res =
        stage === 'coo'
          ? await cooApi.pending(params)
          : stage === 'safety'
            ? await verificationApi.safetyQueue(params)
            : stage === 'department-head'
              ? await verificationApi.deptHeadQueue(params)
              : await verificationApi.qaQueue(params);
      setRows(res.data.data?.items || []);
      setTotalPages(res.data.data?.totalPages || 1);
    } catch {
      toast.error('Failed to load queue');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, stage]);

  return (
    <div className="page">
      <section className="page-intro">
        <h2>{title}</h2>
        <p>Assigned requests awaiting your review. View-only for ER content; approve or reject with comments.</p>
      </section>
      <section className="filters">
        <input placeholder="Search" value={search} onChange={(e) => setSearch(e.target.value)} />
        <button
          type="button"
          className="primary"
          onClick={() => {
            setPage(1);
            void load();
          }}
        >
          Search
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
              { key: 'er', header: 'ER Number', render: (r) => r.erNumber },
              { key: 'title', header: 'Title', render: (r) => r.title },
              { key: 'requester', header: 'Requester', render: (r) => r.requesterName },
              { key: 'dept', header: 'Department', render: (r) => r.departmentName },
              {
                key: 'status',
                header: 'Status',
                render: (r) => <StatusBadge status={r.status} label={r.statusName} />,
              },
              {
                key: 'actions',
                header: 'Actions',
                render: (r) => (
                  <button type="button" onClick={(e) => { e.stopPropagation(); navigate(`/requests/${r.id}`); }}>
                    Review
                  </button>
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

export function NotificationsPage() {
  const navigate = useNavigate();
  const [items, setItems] = useState<NotificationItem[]>([]);
  const [loading, setLoading] = useState(true);

  const load = async () => {
    setLoading(true);
    try {
      const res = await notificationApi.list({ page: 1, pageSize: 50 });
      setItems(res.data.data?.items || []);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void load();
  }, []);

  const markAll = async () => {
    await notificationApi.markAllRead();
    toast.success('All notifications marked as read');
    await load();
  };

  return (
    <div className="page">
      <section className="page-intro row">
        <div>
          <h2>Notifications</h2>
          <p>Submission, assignment, approval, rejection, and resubmission alerts.</p>
        </div>
        <button type="button" className="ghost" onClick={() => void markAll()}>
          Mark all read
        </button>
      </section>
      {loading ? (
        <LoadingBlock />
      ) : (
        <ul className="notification-list">
          {items.map((n) => (
            <li
              key={n.id}
              className={n.isRead ? '' : 'unread'}
              onClick={async () => {
                if (!n.isRead) await notificationApi.markRead(n.id);
                if (n.engineeringRequestId) navigate(`/requests/${n.engineeringRequestId}`);
                await load();
              }}
            >
              <div>
                <strong>{n.subject}</strong>
                <p>{n.message}</p>
              </div>
              <span>{new Date(n.createdDate).toLocaleString()}</span>
            </li>
          ))}
          {!items.length && <li className="muted">No notifications.</li>}
        </ul>
      )}
    </div>
  );
}
