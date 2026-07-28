import type { ReactNode } from 'react';
import type { ErStatus } from '../types';

const statusClass: Record<string, string> = {
  Draft: 'st-draft',
  Submitted: 'st-submitted',
  InProgress: 'st-progress',
  AwaitingCooApproval: 'st-coo',
  Approved: 'st-approved',
  Rejected: 'st-rejected',
  Resubmitted: 'st-resubmit',
  Closed: 'st-closed',
};

export function StatusBadge({ status, label }: { status: ErStatus | string; label?: string }) {
  return <span className={`status-badge ${statusClass[status] || ''}`}>{label || status}</span>;
}

export function LoadingBlock({ label = 'Loading...' }: { label?: string }) {
  return (
    <div className="loading-block">
      <div className="spinner" />
      <p>{label}</p>
    </div>
  );
}

export function ConfirmDialog({
  open,
  title,
  message,
  confirmLabel = 'Confirm',
  onConfirm,
  onCancel,
}: {
  open: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  onConfirm: () => void;
  onCancel: () => void;
}) {
  if (!open) return null;
  return (
    <div className="modal-backdrop" role="presentation" onClick={onCancel}>
      <div className="modal" role="dialog" onClick={(e) => e.stopPropagation()}>
        <h3>{title}</h3>
        <p>{message}</p>
        <div className="modal-actions">
          <button type="button" className="ghost" onClick={onCancel}>
            Cancel
          </button>
          <button type="button" className="primary" onClick={onConfirm}>
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}

export function DataTable<T extends { id: string }>({
  columns,
  rows,
  empty = 'No records found.',
  onRowClick,
}: {
  columns: Array<{ key: string; header: string; render: (row: T) => ReactNode }>;
  rows: T[];
  empty?: string;
  onRowClick?: (row: T) => void;
}) {
  if (!rows.length) return <div className="empty-state">{empty}</div>;
  return (
    <div className="table-wrap">
      <table>
        <thead>
          <tr>
            {columns.map((c) => (
              <th key={c.key}>{c.header}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.id} onClick={() => onRowClick?.(row)} className={onRowClick ? 'clickable' : undefined}>
              {columns.map((c) => (
                <td key={c.key}>{c.render(row)}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function Pagination({
  page,
  totalPages,
  onChange,
}: {
  page: number;
  totalPages: number;
  onChange: (page: number) => void;
}) {
  if (totalPages <= 1) return null;
  return (
    <div className="pagination">
      <button type="button" disabled={page <= 1} onClick={() => onChange(page - 1)}>
        Previous
      </button>
      <span>
        Page {page} of {totalPages}
      </span>
      <button type="button" disabled={page >= totalPages} onClick={() => onChange(page + 1)}>
        Next
      </button>
    </div>
  );
}
