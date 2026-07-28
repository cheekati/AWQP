import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import toast from 'react-hot-toast';
import { attachmentApi, cooApi, erApi, verificationApi } from '../api/services';
import type { EngineeringRequestDetail, VerificationAction } from '../types';
import { ConfirmDialog, LoadingBlock, StatusBadge } from '../components/Ui';
import { AuthImage } from '../hooks/useAuthFileUrl';
import { useAuth } from '../auth/AuthContext';

export function RequestDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { hasRole, user } = useAuth();
  const [data, setData] = useState<EngineeringRequestDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [comments, setComments] = useState('');
  const [preview, setPreview] = useState<string | null>(null);
  const [pendingAction, setPendingAction] = useState<{
    action: VerificationAction;
    stage?: 'safety' | 'department-head' | 'qa' | 'coo';
  } | null>(null);

  const load = async () => {
    if (!id) return;
    setLoading(true);
    try {
      const res = await erApi.get(id);
      setData(res.data.data || null);
    } catch {
      toast.error('Failed to load request');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  const canEdit =
    data &&
    user?.id === data.requesterId &&
    ['Draft', 'Rejected', 'Resubmitted'].includes(data.status);

  const runAction = async () => {
    if (!id || !pendingAction) return;
    if (
      (pendingAction.action === 'Rejected' || pendingAction.action === 'Resubmit') &&
      !comments.trim()
    ) {
      toast.error('Comments are mandatory for reject/resubmit');
      return;
    }
    try {
      if (pendingAction.stage === 'coo') {
        await cooApi.act(id, pendingAction.action, comments);
      } else if (pendingAction.stage) {
        await verificationApi.act(id, pendingAction.stage, pendingAction.action, comments);
      }
      toast.success(`${pendingAction.action} recorded`);
      setPendingAction(null);
      setComments('');
      await load();
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string; errors?: string[] } } })?.response?.data
          ?.errors?.[0] ||
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'Action failed';
      toast.error(msg);
    }
  };

  if (loading) return <LoadingBlock />;
  if (!data) return <div className="empty-state">Request not found.</div>;

  const showSafety = hasRole('Safety') && data.status === 'InProgress';
  const showDept = hasRole('DepartmentHead') && data.status === 'InProgress';
  const showQa = hasRole('QA') && data.status === 'InProgress';
  const showCoo = hasRole('COO') && data.status === 'AwaitingCooApproval';

  return (
    <div className="page">
      <section className="page-intro row">
        <div>
          <p className="eyebrow">{data.erNumber}</p>
          <h2>{data.title}</h2>
          <StatusBadge status={data.status} label={data.statusName} />
        </div>
        <div className="row-actions">
          <button type="button" className="ghost" onClick={() => navigate(-1)}>
            Back
          </button>
          {canEdit && (
            <Link className="primary" to={`/requests/${data.id}/edit`}>
              Edit
            </Link>
          )}
        </div>
      </section>

      {data.latestRejectionComments && (
        <div className="alert danger">
          <strong>Latest comments:</strong> {data.latestRejectionComments}
        </div>
      )}

      <section className="detail-grid">
        <article>
          <h3>Request Summary</h3>
          <dl>
            <div><dt>Division</dt><dd>{data.divisionName}</dd></div>
            <div><dt>Department</dt><dd>{data.departmentName}</dd></div>
            <div><dt>Product</dt><dd>{data.productName}</dd></div>
            <div><dt>Customer</dt><dd>{data.customer}</dd></div>
            <div><dt>Requester</dt><dd>{data.requesterName}</dd></div>
            <div><dt>Submission Count</dt><dd>{data.submissionCount} / 2</dd></div>
            <div><dt>Validation Date</dt><dd>{new Date(data.validationDate).toLocaleString()}</dd></div>
            <div><dt>Expiry Date</dt><dd>{new Date(data.expiryDate).toLocaleString()}</dd></div>
            <div><dt>Category</dt><dd>{[data.isPr && 'PR', data.isEng && 'ENG', data.isQa && 'QA', data.isInd && 'IND'].filter(Boolean).join(', ') || '-'}</dd></div>
            <div><dt>Reasons</dt><dd>{data.reasons.join(', ')}</dd></div>
          </dl>
        </article>
        <article>
          <h3>Evaluation</h3>
          <p><strong>Process:</strong> {data.process}</p>
          <p><strong>Details:</strong> {data.detailsOfEvaluation || '-'}</p>
          <p><strong>Present:</strong> {data.presentCondition || '-'}</p>
          <p><strong>New:</strong> {data.newCondition || '-'}</p>
          <p><strong>Merit:</strong> {data.merit || '-'}</p>
          <p><strong>Demerit:</strong> {data.demerit || '-'}</p>
          <p><strong>Material Disposition:</strong> {data.materialDisposition || '-'}</p>
          <p><strong>Sample Qty:</strong> {data.sampleQuantity ?? '-'}</p>
          <p><strong>Test Lot:</strong> {data.testLotIdentification || '-'} {data.testLotDescription ? `— ${data.testLotDescription}` : ''}</p>
          {data.applicableToChemicalOrMaterials && (
            <>
              <p><strong>SDS:</strong> {data.safetyDataSheet}</p>
              <p><strong>Label:</strong> {data.chemicalLabel}</p>
              <p><strong>Classification:</strong> {data.chemicalClassification}</p>
              <p><strong>CIMS:</strong> {data.chemicalInventoryManagementSystem}</p>
            </>
          )}
        </article>
      </section>

      <section className="panel">
        <h3>Attachments</h3>
        <div className="attachment-grid">
          {data.attachments.map((a) => (
            <article key={a.id}>
              {a.isImage ? (
                <button type="button" className="thumb" onClick={() => setPreview(attachmentApi.downloadUrl(data.id, a.id))}>
                  <AuthImage downloadPath={attachmentApi.downloadUrl(data.id, a.id)} alt={a.originalFileName} />
                </button>
              ) : (
                <div className="file-chip">{a.originalFileName}</div>
              )}
              <a href={attachmentApi.downloadUrl(data.id, a.id)} target="_blank" rel="noreferrer">
                Download
              </a>
            </article>
          ))}
          {!data.attachments.length && <p className="muted">No attachments.</p>}
        </div>
      </section>

      <div className="split">
        <section className="panel">
          <h3>Verification History</h3>
          <ul className="timeline">
            {data.verificationHistories.map((h) => (
              <li key={h.id}>
                <strong>{h.stageName}</strong> — {h.actionName} by {h.verifierName}
                <span>{new Date(h.actionDate).toLocaleString()}</span>
                {h.comments && <p>{h.comments}</p>}
              </li>
            ))}
            {!data.verificationHistories.length && <li className="muted">No verifications yet.</li>}
          </ul>
        </section>
        <section className="panel">
          <h3>COO Approval History</h3>
          <ul className="timeline">
            {data.approvalHistories.map((h) => (
              <li key={h.id}>
                <strong>{h.actionName}</strong> by {h.approverName}
                <span>{new Date(h.actionDate).toLocaleString()}</span>
                {h.comments && <p>{h.comments}</p>}
              </li>
            ))}
            {!data.approvalHistories.length && <li className="muted">No COO actions yet.</li>}
          </ul>
        </section>
      </div>

      {(showSafety || showDept || showQa || showCoo) && (
        <section className="panel action-panel">
          <h3>Actions</h3>
          <p>Verifiers cannot modify the Engineering Request content. Approve, reject, or comment only.</p>
          <label>
            Comments
            <textarea rows={3} value={comments} onChange={(e) => setComments(e.target.value)} />
          </label>
          <div className="row-actions">
            {showSafety && (
              <>
                <button type="button" className="primary" onClick={() => setPendingAction({ action: 'Approved', stage: 'safety' })}>Approve (Safety)</button>
                <button type="button" className="danger" onClick={() => setPendingAction({ action: 'Rejected', stage: 'safety' })}>Reject</button>
              </>
            )}
            {showDept && (
              <>
                <button type="button" className="primary" onClick={() => setPendingAction({ action: 'Approved', stage: 'department-head' })}>Approve (Dept Head)</button>
                <button type="button" className="danger" onClick={() => setPendingAction({ action: 'Rejected', stage: 'department-head' })}>Reject</button>
              </>
            )}
            {showQa && (
              <>
                <button type="button" className="primary" onClick={() => setPendingAction({ action: 'Approved', stage: 'qa' })}>Approve (QA)</button>
                <button type="button" className="danger" onClick={() => setPendingAction({ action: 'Rejected', stage: 'qa' })}>Reject</button>
              </>
            )}
            {showCoo && (
              <>
                <button type="button" className="primary" onClick={() => setPendingAction({ action: 'Approved', stage: 'coo' })}>Approve</button>
                <button type="button" className="danger" onClick={() => setPendingAction({ action: 'Rejected', stage: 'coo' })}>Reject</button>
                <button type="button" onClick={() => setPendingAction({ action: 'Resubmit', stage: 'coo' })}>Resubmit</button>
              </>
            )}
          </div>
        </section>
      )}

      <ConfirmDialog
        open={!!pendingAction}
        title={`${pendingAction?.action}?`}
        message="This action will update the workflow status and notify relevant users."
        confirmLabel="Confirm"
        onCancel={() => setPendingAction(null)}
        onConfirm={() => void runAction()}
      />

      {preview && (
        <div className="modal-backdrop" onClick={() => setPreview(null)} role="presentation">
          <div className="modal image-modal" onClick={(e) => e.stopPropagation()} role="dialog">
            <AuthImage downloadPath={preview} alt="Attachment preview" />
            <button type="button" className="primary" onClick={() => setPreview(null)}>Close</button>
          </div>
        </div>
      )}
    </div>
  );
}
