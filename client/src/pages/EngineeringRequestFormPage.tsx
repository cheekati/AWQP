import type { FormEvent } from 'react';
import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import toast from 'react-hot-toast';
import { attachmentApi, erApi, masterDataApi } from '../api/services';
import type { Attachment, ErFormData, LookupItem, ReasonForChange, TestLotIdentification } from '../types';
import { emptyErForm } from '../types';
import { ConfirmDialog, LoadingBlock } from '../components/Ui';
import { AuthImage } from '../hooks/useAuthFileUrl';

export function EngineeringRequestFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const { id } = useParams();
  const navigate = useNavigate();
  const [form, setForm] = useState<ErFormData>(emptyErForm());
  const [erNumber, setErNumber] = useState('Auto-generated');
  const [validationDate, setValidationDate] = useState<string>('');
  const [lookups, setLookups] = useState<{
    departments: LookupItem[];
    divisions: LookupItem[];
    products: LookupItem[];
  }>({ departments: [], divisions: [], products: [] });
  const [attachments, setAttachments] = useState<Attachment[]>([]);
  const [preview, setPreview] = useState<Attachment | null>(null);
  const [loading, setLoading] = useState(mode === 'edit');
  const [busy, setBusy] = useState(false);
  const [confirmSubmit, setConfirmSubmit] = useState(false);
  const [rejectionComments, setRejectionComments] = useState<string | undefined>();

  useEffect(() => {
    const boot = async () => {
      const [d, v, p] = await Promise.all([
        masterDataApi.departments(),
        masterDataApi.divisions(),
        masterDataApi.products(),
      ]);
      setLookups({
        departments: d.data.data || [],
        divisions: v.data.data || [],
        products: p.data.data || [],
      });

      if (mode === 'edit' && id) {
        const res = await erApi.get(id);
        const data = res.data.data;
        if (!data) {
          toast.error('Request not found');
          navigate('/requests');
          return;
        }
        setErNumber(data.erNumber);
        setValidationDate(data.validationDate);
        setRejectionComments(data.latestRejectionComments);
        setAttachments(data.attachments || []);
        setForm({
          divisionId: data.divisionId,
          isPr: data.isPr,
          isEng: data.isEng,
          isQa: data.isQa,
          isInd: data.isInd,
          title: data.title,
          departmentId: data.departmentId,
          productId: data.productId,
          customer: data.customer,
          process: data.process,
          detailsOfEvaluation: data.detailsOfEvaluation || '',
          reasons: data.reasons || [],
          otherReasonDescription: data.otherReasonDescription || '',
          presentCondition: data.presentCondition || '',
          newCondition: data.newCondition || '',
          merit: data.merit || '',
          demerit: data.demerit || '',
          materialDisposition: data.materialDisposition || '',
          sampleQuantity: data.sampleQuantity?.toString() ?? '',
          testLotIdentification: data.testLotIdentification || '',
          testLotDescription: data.testLotDescription || '',
          applicableToChemicalOrMaterials: data.applicableToChemicalOrMaterials,
          safetyDataSheet: data.safetyDataSheet || '',
          chemicalLabel: data.chemicalLabel || '',
          chemicalClassification: data.chemicalClassification || '',
          chemicalInventoryManagementSystem: data.chemicalInventoryManagementSystem || '',
          submit: false,
        });
      }
      setLoading(false);
    };
    void boot();
  }, [id, mode, navigate]);

  const setField = <K extends keyof ErFormData>(key: K, value: ErFormData[K]) =>
    setForm((prev) => ({ ...prev, [key]: value }));

  const toggleReason = (reason: ReasonForChange) => {
    setForm((prev) => ({
      ...prev,
      reasons: prev.reasons.includes(reason)
        ? prev.reasons.filter((r) => r !== reason)
        : [...prev.reasons, reason],
    }));
  };

  const validateClient = () => {
    if (!form.title.trim()) return 'Title is required';
    if (!form.divisionId || !form.departmentId || !form.productId) return 'Division, Department and Product are required';
    if (!form.customer.trim() || !form.process.trim()) return 'Customer and Process are required';
    if (!form.reasons.length) return 'Select at least one reason for change';
    if (form.reasons.includes('Others') && !form.otherReasonDescription.trim())
      return 'Other reason description is required';
    if (form.sampleQuantity && !/^\d+$/.test(form.sampleQuantity))
      return 'Sample quantity must contain numbers only';
    if (form.testLotIdentification && !form.testLotDescription.trim())
      return 'Test lot description is mandatory when an option is selected';
    if (form.applicableToChemicalOrMaterials) {
      if (!form.safetyDataSheet || !form.chemicalLabel || !form.chemicalClassification || !form.chemicalInventoryManagementSystem)
        return 'All chemical/material fields are required when applicable';
    }
    return null;
  };

  const save = async (submit: boolean) => {
    const error = validateClient();
    if (error) {
      toast.error(error);
      return;
    }
    setBusy(true);
    try {
      const payload = { ...form, submit };
      const res =
        mode === 'edit' && id
          ? await erApi.update(id, payload)
          : await erApi.create(payload);
      if (!res.data.success || !res.data.data) {
        toast.error(res.data.message || 'Save failed');
        return;
      }
      toast.success(submit ? 'Request submitted' : 'Draft saved');
      navigate('/requests');
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string; errors?: string[] } } })?.response?.data
          ?.errors?.[0] ||
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'Save failed';
      toast.error(msg);
    } finally {
      setBusy(false);
      setConfirmSubmit(false);
    }
  };

  const onUpload = async (files: FileList | null) => {
    if (!files?.length) return;
    if (mode !== 'edit' || !id) {
      toast.error('Save the request as draft first, then upload attachments.');
      return;
    }
    try {
      const res = await attachmentApi.upload(id, files);
      if (res.data.success && res.data.data) {
        setAttachments((prev) => [...prev, ...res.data.data!]);
        toast.success(`${res.data.data.length} file(s) uploaded`);
      }
    } catch {
      toast.error('Upload failed');
    }
  };

  const removeAttachment = async (attachmentId: string) => {
    if (!id) return;
    await attachmentApi.remove(id, attachmentId);
    setAttachments((prev) => prev.filter((a) => a.id !== attachmentId));
    toast.success('Attachment deleted');
  };

  const reasonOptions = useMemo(
    () =>
      [
        ['CostDown', 'Cost Down'],
        ['AlternativeSourcing', 'Alternative Sourcing'],
        ['Others', 'Others'],
      ] as Array<[ReasonForChange, string]>,
    [],
  );

  if (loading) return <LoadingBlock />;

  return (
    <div className="page">
      <section className="page-intro">
        <h2>{mode === 'create' ? 'Create Engineering Request' : 'Edit Engineering Request'}</h2>
        <p>Complete the evaluation details, then save as draft or submit into the verification workflow.</p>
      </section>

      {rejectionComments && (
        <div className="alert danger">
          <strong>Rejection / Resubmit comments:</strong> {rejectionComments}
        </div>
      )}

      <form
        className="er-form"
        onSubmit={(e: FormEvent) => {
          e.preventDefault();
          void save(false);
        }}
      >
        <section className="form-grid">
          <label>
            ER Number
            <input value={erNumber} readOnly />
          </label>
          <label>
            Validation Date
            <input value={validationDate ? new Date(validationDate).toLocaleString() : 'Auto on create'} readOnly />
          </label>
          <label>
            Division
            <select value={form.divisionId} onChange={(e) => setField('divisionId', e.target.value)} required>
              <option value="">Select</option>
              {lookups.divisions.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Department
            <select value={form.departmentId} onChange={(e) => setField('departmentId', e.target.value)} required>
              <option value="">Select</option>
              {lookups.departments.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Product
            <select value={form.productId} onChange={(e) => setField('productId', e.target.value)} required>
              <option value="">Select</option>
              {lookups.products.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Title
            <input value={form.title} onChange={(e) => setField('title', e.target.value)} required />
          </label>
          <label>
            Customer
            <input value={form.customer} onChange={(e) => setField('customer', e.target.value)} required />
          </label>
          <fieldset className="checks">
            <legend>Category</legend>
            {(
              [
                ['isPr', 'PR'],
                ['isEng', 'ENG'],
                ['isQa', 'QA'],
                ['isInd', 'IND'],
              ] as const
            ).map(([key, label]) => (
              <label key={key} className="check">
                <input
                  type="checkbox"
                  checked={form[key]}
                  onChange={(e) => setField(key, e.target.checked)}
                />
                {label}
              </label>
            ))}
          </fieldset>
        </section>

        <label>
          Process
          <textarea rows={3} value={form.process} onChange={(e) => setField('process', e.target.value)} required />
        </label>
        <label>
          Details of Evaluation
          <textarea
            rows={3}
            value={form.detailsOfEvaluation}
            onChange={(e) => setField('detailsOfEvaluation', e.target.value)}
          />
        </label>

        <fieldset>
          <legend>Reason for Change</legend>
          <div className="checks">
            {reasonOptions.map(([value, label]) => (
              <label key={value} className="check">
                <input
                  type="checkbox"
                  checked={form.reasons.includes(value)}
                  onChange={() => toggleReason(value)}
                />
                {label}
              </label>
            ))}
          </div>
          {form.reasons.includes('Others') && (
            <label>
              Other Description
              <textarea
                rows={2}
                value={form.otherReasonDescription}
                onChange={(e) => setField('otherReasonDescription', e.target.value)}
                required
              />
            </label>
          )}
        </fieldset>

        <div className="form-grid">
          <label>
            Present Condition
            <textarea rows={3} value={form.presentCondition} onChange={(e) => setField('presentCondition', e.target.value)} />
          </label>
          <label>
            New Condition
            <textarea rows={3} value={form.newCondition} onChange={(e) => setField('newCondition', e.target.value)} />
          </label>
          <label>
            Merit
            <textarea rows={3} value={form.merit} onChange={(e) => setField('merit', e.target.value)} />
          </label>
          <label>
            Demerit
            <textarea rows={3} value={form.demerit} onChange={(e) => setField('demerit', e.target.value)} />
          </label>
        </div>

        <label>
          Material Disposition
          <textarea
            rows={2}
            value={form.materialDisposition}
            onChange={(e) => setField('materialDisposition', e.target.value)}
          />
        </label>

        <div className="form-grid">
          <label>
            Sample Quantity
            <input
              inputMode="numeric"
              pattern="[0-9]*"
              value={form.sampleQuantity}
              onChange={(e) => setField('sampleQuantity', e.target.value.replace(/[^\d]/g, ''))}
              placeholder="Numbers only"
            />
          </label>
          <label>
            Identification Of Test Lot
            <select
              value={form.testLotIdentification}
              onChange={(e) =>
                setField('testLotIdentification', e.target.value as TestLotIdentification | '')
              }
            >
              <option value="">Select</option>
              <option value="RunAsNormal">1. RUN AS NORMAL</option>
              <option value="SpecialProcess">2. SPECIAL PROCESS</option>
              <option value="Others">3. OTHERS</option>
            </select>
          </label>
        </div>
        {form.testLotIdentification && (
          <label>
            Test Lot Description
            <textarea
              rows={2}
              value={form.testLotDescription}
              onChange={(e) => setField('testLotDescription', e.target.value)}
              required
            />
          </label>
        )}

        <label className="check">
          <input
            type="checkbox"
            checked={form.applicableToChemicalOrMaterials}
            onChange={(e) => setField('applicableToChemicalOrMaterials', e.target.checked)}
          />
          Applicable to Any Chemical or Materials
        </label>

        {form.applicableToChemicalOrMaterials && (
          <div className="form-grid">
            <label>
              Safety Data Sheet
              <input value={form.safetyDataSheet} onChange={(e) => setField('safetyDataSheet', e.target.value)} required />
            </label>
            <label>
              Chemical Label
              <input value={form.chemicalLabel} onChange={(e) => setField('chemicalLabel', e.target.value)} required />
            </label>
            <label>
              Chemical Classification
              <input
                value={form.chemicalClassification}
                onChange={(e) => setField('chemicalClassification', e.target.value)}
                required
              />
            </label>
            <label>
              Chemical Inventory Management System (CIMS)
              <input
                value={form.chemicalInventoryManagementSystem}
                onChange={(e) => setField('chemicalInventoryManagementSystem', e.target.value)}
                required
              />
            </label>
          </div>
        )}

        <section className="attachments">
          <div className="panel-head">
            <h3>Attachments</h3>
            <p>Multiple files and 100+ images supported. Additional selections append to existing uploads.</p>
          </div>
          {mode === 'create' ? (
            <div className="alert">Save as draft first to enable uploads.</div>
          ) : (
            <input type="file" multiple onChange={(e) => void onUpload(e.target.files)} />
          )}
          <div className="attachment-grid">
            {attachments.map((a) => (
              <article key={a.id}>
                {a.isImage ? (
                  <button type="button" className="thumb" onClick={() => setPreview(a)}>
                    <AuthImage downloadPath={attachmentApi.downloadUrl(id!, a.id)} alt={a.originalFileName} />
                  </button>
                ) : (
                  <div className="file-chip">{a.originalFileName}</div>
                )}
                <div className="row-actions">
                  <a href={attachmentApi.downloadUrl(id!, a.id)} target="_blank" rel="noreferrer">
                    Download
                  </a>
                  <button type="button" onClick={() => void removeAttachment(a.id)}>
                    Delete
                  </button>
                </div>
              </article>
            ))}
          </div>
        </section>

        <div className="form-actions">
          <button type="submit" className="ghost" disabled={busy}>
            Save Draft
          </button>
          <button type="button" className="primary" disabled={busy} onClick={() => setConfirmSubmit(true)}>
            Submit
          </button>
        </div>
      </form>

      <ConfirmDialog
        open={confirmSubmit}
        title="Submit Engineering Request?"
        message="The request will move to In Progress and notify Safety, Department Head, and QA verifiers."
        confirmLabel="Submit"
        onCancel={() => setConfirmSubmit(false)}
        onConfirm={() => void save(true)}
      />

      {preview && (
        <div className="modal-backdrop" onClick={() => setPreview(null)} role="presentation">
          <div className="modal image-modal" onClick={(e) => e.stopPropagation()} role="dialog">
            <AuthImage downloadPath={attachmentApi.downloadUrl(id!, preview.id)} alt={preview.originalFileName} />
            <button type="button" className="primary" onClick={() => setPreview(null)}>
              Close
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
