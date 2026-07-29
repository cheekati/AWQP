import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { erApi } from '../api/client';
import AttachmentUploader from '../components/AttachmentUploader';
import StatusBadge from '../components/StatusBadge';
import type { ErDetail } from '../types';

export default function ErDetailPage() {
  const { id } = useParams();
  const [er, setEr] = useState<ErDetail | null>(null);
  const [error, setError] = useState('');
  const [comment, setComment] = useState('');
  const [outcome, setOutcome] = useState(1);
  const [busy, setBusy] = useState(false);

  async function load() {
    try {
      const data = await erApi.get(Number(id));
      setEr(data);
    } catch {
      setError('Failed to load ER.');
    }
  }

  useEffect(() => {
    void load();
  }, [id]);

  async function verify(decision: number) {
    if (!er) return;
    setBusy(true);
    try {
      const updated = await erApi.verify(er.id, decision, comment);
      setEr(updated);
      setComment('');
    } catch (err: unknown) {
      setError(
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
          'Verification failed.',
      );
    } finally {
      setBusy(false);
    }
  }

  async function coo(decision: number) {
    if (!er) return;
    setBusy(true);
    try {
      const updated = await erApi.coo(er.id, decision, comment, decision === 1 ? outcome : undefined);
      setEr(updated);
      setComment('');
    } catch (err: unknown) {
      setError(
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
          'COO action failed.',
      );
    } finally {
      setBusy(false);
    }
  }

  if (error && !er) return <p className="error">{error}</p>;
  if (!er) return <p>Loading…</p>;

  return (
    <section className="panel">
      <div className="panel-head">
        <div>
          <p className="eyebrow">Engineering Request</p>
          <h1 className="mono">{er.displayErNumber}</h1>
          <p className="lede">{er.title}</p>
        </div>
        <div className="row-actions">
          <StatusBadge status={er.status} />
          {er.canEdit && (
            <Link className="btn" to={`/ers/${er.id}/edit`}>
              Edit
            </Link>
          )}
          <Link className="btn ghost" to="/">
            Back
          </Link>
        </div>
      </div>

      <div className="detail-grid">
        <div className="detail-block">
          <h2>Header</h2>
          <dl>
            <dt>Division</dt>
            <dd>{er.division}</dd>
            <dt>Department</dt>
            <dd>{er.department}</dd>
            <dt>Product</dt>
            <dd>{er.product}</dd>
            <dt>Customer</dt>
            <dd>{er.customer || '—'}</dd>
            <dt>Process</dt>
            <dd>{er.process || '—'}</dd>
            <dt>Validation Date</dt>
            <dd>{new Date(er.validationDate).toLocaleString()}</dd>
            <dt>Valid Until</dt>
            <dd>
              {new Date(er.submissionValidUntil).toLocaleDateString()} ({er.daysRemaining} days)
            </dd>
            <dt>Submission</dt>
            <dd>
              {er.submissionNumber}/2
            </dd>
            <dt>Requester</dt>
            <dd>{er.requesterName}</dd>
          </dl>
        </div>

        <div className="detail-block">
          <h2>Evaluation</h2>
          <p>
            Reason:{' '}
            {[
              er.reasonCostDown && 'Cost Down',
              er.reasonAlternativeSourcing && 'Alternative Sourcing',
              er.reasonOthers && `Others${er.reasonOthersText ? `: ${er.reasonOthersText}` : ''}`,
            ]
              .filter(Boolean)
              .join(' · ') || '—'}
          </p>
          <div className="grid-2">
            <div>
              <h3>Present</h3>
              <p className="pre">{er.presentDetails || '—'}</p>
            </div>
            <div>
              <h3>New</h3>
              <p className="pre">{er.newDetails || '—'}</p>
            </div>
            <div>
              <h3>Merit</h3>
              <p className="pre">{er.merit || '—'}</p>
            </div>
            <div>
              <h3>Demerit</h3>
              <p className="pre">{er.demerit || '—'}</p>
            </div>
          </div>
        </div>

        <div className="detail-block">
          <h2>Material Disposition</h2>
          <p className="pre">{er.materialDisposition || '—'}</p>
          <dl>
            <dt>Sample Quantity</dt>
            <dd>{er.sampleQuantity ?? '—'}</dd>
            <dt>Test Lot Code/Serial</dt>
            <dd>{er.testLotCodeSerial || '—'}</dd>
            <dt>Test Lot Type</dt>
            <dd>{er.testLotType || '—'}</dd>
            <dt>Description</dt>
            <dd>{er.testLotDescription || '—'}</dd>
          </dl>
        </div>

        {er.applicableToChemicalOrMaterials && (
          <div className="detail-block">
            <h2>Chemical / Materials</h2>
            <dl>
              <dt>Safety Data Sheet</dt>
              <dd>{er.safetyDataSheet || '—'}</dd>
              <dt>Chemical Label</dt>
              <dd>{er.chemicalLabel || '—'}</dd>
              <dt>Classification</dt>
              <dd>{er.chemicalClassification || '—'}</dd>
              <dt>CIMS</dt>
              <dd>{er.chemicalInventorySystem || '—'}</dd>
            </dl>
          </div>
        )}

        <div className="detail-block">
          <h2>Attachments</h2>
          <AttachmentUploader
            erId={er.id}
            attachments={er.attachments}
            onChange={(items) => setEr({ ...er, attachments: items })}
            editable={er.canEdit}
          />
        </div>

        <div className="detail-block">
          <h2>Verification</h2>
          <table className="compact">
            <thead>
              <tr>
                <th>Level</th>
                <th>Assignee</th>
                <th>Decision</th>
                <th>Comment</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>Safety</td>
                <td>{er.verifiedBySafetyName || '—'}</td>
                <td>{er.safetyDecision}</td>
                <td>{er.safetyComment || '—'}</td>
              </tr>
              <tr>
                <td>Dept Head</td>
                <td>{er.checkedByDeptHeadName || '—'}</td>
                <td>{er.deptHeadDecision}</td>
                <td>{er.deptHeadComment || '—'}</td>
              </tr>
              <tr>
                <td>QA</td>
                <td>{er.checkedByQaName || '—'}</td>
                <td>{er.qaDecision}</td>
                <td>{er.qaComment || '—'}</td>
              </tr>
              <tr>
                <td>COO</td>
                <td>—</td>
                <td>{er.cooDecision}</td>
                <td>{er.cooComment || '—'}</td>
              </tr>
            </tbody>
          </table>
          <p>
            ER Outcome: <strong>{er.outcome === 'None' ? '—' : er.outcome}</strong>
          </p>
        </div>

        {(er.canVerify || er.canCooAct) && (
          <div className="detail-block action-panel">
            <h2>{er.canCooAct ? 'COO Decision' : 'Verify (Approve / Cancel)'}</h2>
            <p className="hint">
              Verifiers cannot modify the request — only approve or reject with a comment.
            </p>
            <label>
              Comment
              <textarea rows={3} value={comment} onChange={(e) => setComment(e.target.value)} />
            </label>
            {er.canCooAct && (
              <div className="check-row">
                <span>ER Status</span>
                <label className="check">
                  <input
                    type="radio"
                    name="outcome"
                    checked={outcome === 1}
                    onChange={() => setOutcome(1)}
                  />
                  Pass
                </label>
                <label className="check">
                  <input
                    type="radio"
                    name="outcome"
                    checked={outcome === 2}
                    onChange={() => setOutcome(2)}
                  />
                  Fail
                </label>
                <label className="check">
                  <input
                    type="radio"
                    name="outcome"
                    checked={outcome === 3}
                    onChange={() => setOutcome(3)}
                  />
                  Re-Submit
                </label>
              </div>
            )}
            <div className="form-actions">
              {er.canVerify && (
                <>
                  <button
                    type="button"
                    className="btn primary"
                    disabled={busy}
                    onClick={() => void verify(1)}
                  >
                    Approve
                  </button>
                  <button
                    type="button"
                    className="btn danger"
                    disabled={busy}
                    onClick={() => void verify(2)}
                  >
                    Reject / Cancel
                  </button>
                </>
              )}
              {er.canCooAct && (
                <>
                  <button
                    type="button"
                    className="btn primary"
                    disabled={busy}
                    onClick={() => void coo(1)}
                  >
                    Approve
                  </button>
                  <button
                    type="button"
                    className="btn danger"
                    disabled={busy}
                    onClick={() => void coo(2)}
                  >
                    Reject
                  </button>
                  <button
                    type="button"
                    className="btn"
                    disabled={busy}
                    onClick={() => void coo(3)}
                  >
                    Re-Submit
                  </button>
                </>
              )}
            </div>
          </div>
        )}

        <div className="detail-block">
          <h2>Activity</h2>
          {er.comments.length === 0 && <p className="muted">No comments yet.</p>}
          <ul className="timeline">
            {er.comments.map((c) => (
              <li key={c.id}>
                <strong>{c.userName}</strong> · {c.action}
                <span>{new Date(c.createdAt).toLocaleString()}</span>
                <p>{c.comment || '—'}</p>
              </li>
            ))}
          </ul>
        </div>
      </div>
      {error && <p className="error">{error}</p>}
    </section>
  );
}
