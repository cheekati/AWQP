import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { erApi, lookupsApi, usersApi } from '../api/client';
import AttachmentUploader from '../components/AttachmentUploader';
import {
  DIVISIONS,
  TEST_LOT_TYPES,
  emptyForm,
  type Attachment,
  type ErFormData,
  type User,
} from '../types';

export default function ErFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const { id } = useParams();
  const navigate = useNavigate();
  const [form, setForm] = useState<ErFormData>(emptyForm());
  const [departments, setDepartments] = useState<string[]>([]);
  const [products, setProducts] = useState<string[]>([]);
  const [safetyUsers, setSafetyUsers] = useState<User[]>([]);
  const [deptUsers, setDeptUsers] = useState<User[]>([]);
  const [qaUsers, setQaUsers] = useState<User[]>([]);
  const [attachments, setAttachments] = useState<Attachment[]>([]);
  const [erId, setErId] = useState<number | null>(mode === 'edit' && id ? Number(id) : null);
  const [displayEr, setDisplayEr] = useState<string>('AUTO GENERATED');
  const [meta, setMeta] = useState<{ days?: number; submission?: number; status?: string }>({});
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    void Promise.all([
      lookupsApi.all(),
      usersApi.list('Safety'),
      usersApi.list('DeptHead'),
      usersApi.list('QA'),
    ]).then(([lookups, safety, dept, qa]) => {
      setDepartments(lookups.find((l) => l.category === 'Department')?.values ?? []);
      setProducts(lookups.find((l) => l.category === 'Product')?.values ?? []);
      setSafetyUsers(safety);
      setDeptUsers(dept);
      setQaUsers(qa);
    });
  }, []);

  useEffect(() => {
    if (mode !== 'edit' || !id) return;
    erApi
      .get(Number(id))
      .then((er) => {
        setErId(er.id);
        setDisplayEr(er.displayErNumber);
        setMeta({ days: er.daysRemaining, submission: er.submissionNumber, status: er.status });
        setAttachments(er.attachments);
        setForm({
          division: DIVISIONS.find((d) => d.label === er.division)?.value ?? 2,
          title: er.title,
          department: er.department,
          product: er.product,
          customer: er.customer,
          process: er.process,
          reasonCostDown: er.reasonCostDown,
          reasonAlternativeSourcing: er.reasonAlternativeSourcing,
          reasonOthers: er.reasonOthers,
          reasonOthersText: er.reasonOthersText ?? '',
          presentDetails: er.presentDetails ?? '',
          newDetails: er.newDetails ?? '',
          merit: er.merit ?? '',
          demerit: er.demerit ?? '',
          materialDisposition: er.materialDisposition ?? '',
          sampleQuantity: er.sampleQuantity?.toString() ?? '',
          testLotType: lotTypeToValue(er.testLotType),
          testLotDescription: er.testLotDescription ?? '',
          testLotCodeSerial: er.testLotCodeSerial ?? '',
          applicableToChemicalOrMaterials: er.applicableToChemicalOrMaterials,
          safetyDataSheet: er.safetyDataSheet ?? '',
          chemicalLabel: er.chemicalLabel ?? '',
          chemicalClassification: er.chemicalClassification ?? '',
          chemicalInventorySystem: er.chemicalInventorySystem ?? '',
          verifiedBySafetyUserId: er.verifiedBySafetyUserId ?? '',
          checkedByDeptHeadUserId: er.checkedByDeptHeadUserId ?? '',
          checkedByQaUserId: er.checkedByQaUserId ?? '',
          submit: false,
        });
      })
      .catch(() => setError('Failed to load ER.'));
  }, [mode, id]);

  const set = <K extends keyof ErFormData>(key: K, value: ErFormData[K]) =>
    setForm((f) => ({ ...f, [key]: value }));

  const canSaveDraft = useMemo(() => form.title.trim().length > 0, [form.title]);

  async function persist(submit: boolean) {
    setError('');
    if (submit) {
      if (!form.reasonCostDown && !form.reasonAlternativeSourcing && !form.reasonOthers) {
        setError('Select at least one Reason for Change.');
        return;
      }
      if (!form.verifiedBySafetyUserId || !form.checkedByDeptHeadUserId || !form.checkedByQaUserId) {
        setError('Select all three verification assignees before submit.');
        return;
      }
    }
    if (form.sampleQuantity !== '' && !/^\d+$/.test(form.sampleQuantity)) {
      setError('Sample quantity: numbers only.');
      return;
    }

    setSaving(true);
    try {
      const payload = { ...form, submit };
      let result;
      if (erId) {
        result = await erApi.update(erId, payload);
      } else {
        result = await erApi.create(payload);
        setErId(result.id);
      }
      setDisplayEr(result.displayErNumber);
      setAttachments(result.attachments);
      if (submit) {
        navigate(`/ers/${result.id}`);
      } else if (mode === 'create') {
        navigate(`/ers/${result.id}/edit`, { replace: true });
      }
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
        'Save failed.';
      setError(msg);
    } finally {
      setSaving(false);
    }
  }

  function onSubmit(e: FormEvent) {
    e.preventDefault();
    void persist(true);
  }

  return (
    <section className="panel form-panel">
      <div className="panel-head">
        <div>
          <p className="eyebrow">Engineering Request</p>
          <h1>{mode === 'create' ? 'Create ER' : 'Edit ER'}</h1>
          <p className="lede">
            Submission valid for 3 months · max 2 submissions · resubmit window 89 days
          </p>
        </div>
        <div className="er-meta">
          <div>
            <span>ER NO</span>
            <strong className="mono">{displayEr}</strong>
          </div>
          {meta.submission != null && (
            <div>
              <span>Submission</span>
              <strong>
                {meta.submission}/2
                {meta.days != null ? ` · ${meta.days}d left` : ''}
              </strong>
            </div>
          )}
          {meta.status && (
            <div>
              <span>Status</span>
              <strong>{meta.status}</strong>
            </div>
          )}
        </div>
      </div>

      <form className="er-form" onSubmit={onSubmit}>
        <fieldset>
          <legend>Header</legend>
          <div className="grid-3">
            <label>
              Division
              <select
                data-testid="division"
                value={form.division}
                onChange={(e) => set('division', Number(e.target.value))}
                required
              >
                {DIVISIONS.map((d) => (
                  <option key={d.value} value={d.value}>
                    {d.label}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Title
              <input
                data-testid="title"
                value={form.title}
                onChange={(e) => set('title', e.target.value)}
                required
                maxLength={200}
              />
            </label>
            <label>
              Department
              <select
                data-testid="department"
                value={form.department}
                onChange={(e) => set('department', e.target.value)}
                required
              >
                <option value="">Select…</option>
                {departments.map((d) => (
                  <option key={d} value={d}>
                    {d}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Product
              <select
                data-testid="product"
                value={form.product}
                onChange={(e) => set('product', e.target.value)}
                required
              >
                <option value="">Select…</option>
                {products.map((p) => (
                  <option key={p} value={p}>
                    {p}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Customer
              <input
                data-testid="customer"
                value={form.customer}
                onChange={(e) => set('customer', e.target.value)}
              />
            </label>
            <label>
              Process
              <input
                data-testid="process"
                value={form.process}
                onChange={(e) => set('process', e.target.value)}
              />
            </label>
          </div>
        </fieldset>

        <fieldset>
          <legend>Details of Evaluation</legend>
          <div className="check-row">
            <span>Reason for Change</span>
            <label className="check">
              <input
                data-testid="reason-cost-down"
                type="checkbox"
                checked={form.reasonCostDown}
                onChange={(e) => set('reasonCostDown', e.target.checked)}
              />
              Cost Down
            </label>
            <label className="check">
              <input
                data-testid="reason-alt-sourcing"
                type="checkbox"
                checked={form.reasonAlternativeSourcing}
                onChange={(e) => set('reasonAlternativeSourcing', e.target.checked)}
              />
              Alternative Sourcing
            </label>
            <label className="check">
              <input
                data-testid="reason-others"
                type="checkbox"
                checked={form.reasonOthers}
                onChange={(e) => set('reasonOthers', e.target.checked)}
              />
              Others
            </label>
          </div>
          {form.reasonOthers && (
            <label>
              Others (specify)
              <input
                value={form.reasonOthersText}
                onChange={(e) => set('reasonOthersText', e.target.value)}
              />
            </label>
          )}
          <div className="grid-2">
            <label>
              Present
              <textarea
                data-testid="present"
                rows={4}
                value={form.presentDetails}
                onChange={(e) => set('presentDetails', e.target.value)}
              />
            </label>
            <label>
              New
              <textarea
                data-testid="new-details"
                rows={4}
                value={form.newDetails}
                onChange={(e) => set('newDetails', e.target.value)}
              />
            </label>
            <label>
              Merit
              <textarea
                data-testid="merit"
                rows={3}
                value={form.merit}
                onChange={(e) => set('merit', e.target.value)}
              />
            </label>
            <label>
              Demerit
              <textarea
                data-testid="demerit"
                rows={3}
                value={form.demerit}
                onChange={(e) => set('demerit', e.target.value)}
              />
            </label>
          </div>
        </fieldset>

        <fieldset>
          <legend>Material Disposition</legend>
          <div className="grid-2">
            <label>
              Material Disposition
              <textarea
                data-testid="material-disposition"
                rows={3}
                value={form.materialDisposition}
                onChange={(e) => set('materialDisposition', e.target.value)}
              />
            </label>
            <label>
              Sample Quantity (numbers only)
              <input
                data-testid="sample-quantity"
                inputMode="numeric"
                pattern="[0-9]*"
                value={form.sampleQuantity}
                onChange={(e) => {
                  const v = e.target.value;
                  if (v === '' || /^\d+$/.test(v)) set('sampleQuantity', v);
                }}
                placeholder="e.g. 10"
              />
            </label>
            <label>
              Identification of Test Lot (Code / Serial)
              <input
                data-testid="test-lot-code"
                value={form.testLotCodeSerial}
                onChange={(e) => set('testLotCodeSerial', e.target.value)}
              />
            </label>
            <label>
              Test Lot Type
              <select
                data-testid="test-lot-type"
                value={form.testLotType}
                onChange={(e) =>
                  set('testLotType', e.target.value === '' ? '' : Number(e.target.value))
                }
              >
                <option value="">Select…</option>
                {TEST_LOT_TYPES.map((t) => (
                  <option key={t.value} value={t.value}>
                    {t.label}
                  </option>
                ))}
              </select>
            </label>
            <label className="span-2">
              Test Lot Description
              <textarea
                data-testid="test-lot-description"
                rows={2}
                value={form.testLotDescription}
                onChange={(e) => set('testLotDescription', e.target.value)}
              />
            </label>
          </div>
        </fieldset>

        <fieldset>
          <legend>Chemical / Materials</legend>
          <label className="check">
            <input
              data-testid="chemical-applicable"
              type="checkbox"
              checked={form.applicableToChemicalOrMaterials}
              onChange={(e) => set('applicableToChemicalOrMaterials', e.target.checked)}
            />
            Applicable to any chemical or materials
          </label>
          {form.applicableToChemicalOrMaterials && (
            <div className="grid-2 chemical-fields">
              <label>
                Safety Data Sheet
                <input
                  data-testid="safety-data-sheet"
                  value={form.safetyDataSheet}
                  onChange={(e) => set('safetyDataSheet', e.target.value)}
                />
              </label>
              <label>
                Chemical Label
                <input
                  data-testid="chemical-label"
                  value={form.chemicalLabel}
                  onChange={(e) => set('chemicalLabel', e.target.value)}
                />
              </label>
              <label>
                Chemical Classification and Chemical
                <input
                  data-testid="chemical-classification"
                  value={form.chemicalClassification}
                  onChange={(e) => set('chemicalClassification', e.target.value)}
                />
              </label>
              <label>
                Chemical Inventory Management System (CIMS)
                <input
                  data-testid="cims"
                  value={form.chemicalInventorySystem}
                  onChange={(e) => set('chemicalInventorySystem', e.target.value)}
                />
              </label>
            </div>
          )}
        </fieldset>

        <fieldset>
          <legend>Attachments</legend>
          <p className="hint">
            Any file type allowed. Multiple images accumulate — selecting another does not replace
            previous ones. Previews appear as each file uploads.
          </p>
          {!erId && (
            <p className="hint">Save a draft first to enable uploads (ER number will be generated).</p>
          )}
          {erId && (
            <AttachmentUploader
              erId={erId}
              attachments={attachments}
              onChange={setAttachments}
              editable
            />
          )}
        </fieldset>

        <fieldset>
          <legend>Verification Assignees</legend>
          <div className="grid-3">
            <label>
              Verified By (Safety)
              <select
                data-testid="verified-by-safety"
                value={form.verifiedBySafetyUserId}
                onChange={(e) =>
                  set(
                    'verifiedBySafetyUserId',
                    e.target.value === '' ? '' : Number(e.target.value),
                  )
                }
              >
                <option value="">Select…</option>
                {safetyUsers.map((u) => (
                  <option key={u.id} value={u.id}>
                    {u.fullName}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Checked By (Dept Head)
              <select
                data-testid="checked-by-dept"
                value={form.checkedByDeptHeadUserId}
                onChange={(e) =>
                  set(
                    'checkedByDeptHeadUserId',
                    e.target.value === '' ? '' : Number(e.target.value),
                  )
                }
              >
                <option value="">Select…</option>
                {deptUsers.map((u) => (
                  <option key={u.id} value={u.id}>
                    {u.fullName}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Checked By (QA)
              <select
                data-testid="checked-by-qa"
                value={form.checkedByQaUserId}
                onChange={(e) =>
                  set('checkedByQaUserId', e.target.value === '' ? '' : Number(e.target.value))
                }
              >
                <option value="">Select…</option>
                {qaUsers.map((u) => (
                  <option key={u.id} value={u.id}>
                    {u.fullName}
                  </option>
                ))}
              </select>
            </label>
          </div>
        </fieldset>

        {error && <p className="error">{error}</p>}

        <div className="form-actions">
          <Link className="btn ghost" to="/">
            Cancel
          </Link>
          <button
            type="button"
            className="btn"
            disabled={saving || !canSaveDraft}
            onClick={() => void persist(false)}
          >
            Save Draft
          </button>
          <button type="submit" className="btn primary" disabled={saving}>
            {saving ? 'Saving…' : 'Submit'}
          </button>
        </div>
      </form>
    </section>
  );
}

function lotTypeToValue(v?: string): number | '' {
  const lotMap: Record<string, number> = {
    RunAsNormal: 1,
    SpecialProcess: 2,
    Others: 3,
  };
  return v && lotMap[v] ? lotMap[v] : '';
}
