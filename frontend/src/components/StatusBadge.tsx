export default function StatusBadge({ status }: { status: string }) {
  const tone =
    status === 'Approved'
      ? 'ok'
      : status === 'Rejected' || status === 'ResubmitRequested'
        ? 'warn'
        : status === 'InProgress'
          ? 'info'
          : 'neutral';
  return <span className={`badge ${tone}`}>{status}</span>;
}
