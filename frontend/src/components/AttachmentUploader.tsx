import { useRef, useState, type ChangeEvent } from 'react';
import { erApi } from '../api/client';
import type { Attachment } from '../types';

interface Props {
  erId: number;
  attachments: Attachment[];
  onChange: (items: Attachment[]) => void;
  editable?: boolean;
}

interface PendingUpload {
  id: string;
  name: string;
  previewUrl?: string;
  progress: number;
  error?: string;
}

export default function AttachmentUploader({ erId, attachments, onChange, editable }: Props) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [pending, setPending] = useState<PendingUpload[]>([]);
  const [lightbox, setLightbox] = useState<string | null>(null);

  async function uploadSequential(files: File[]) {
    let current = [...attachments];
    for (const file of files) {
      const localId = `${file.name}-${file.size}-${Date.now()}-${Math.random()}`;
      const previewUrl = file.type.startsWith('image/') ? URL.createObjectURL(file) : undefined;
      setPending((p) => [...p, { id: localId, name: file.name, previewUrl, progress: 0 }]);
      try {
        const uploaded = await erApi.upload(erId, file, (pct) => {
          setPending((p) => p.map((x) => (x.id === localId ? { ...x, progress: pct } : x)));
        });
        current = [...current, uploaded];
        onChange(current);
        setPending((p) => p.filter((x) => x.id !== localId));
        if (previewUrl) URL.revokeObjectURL(previewUrl);
        if (uploaded.isImage) setLightbox(uploaded.url);
      } catch {
        setPending((p) =>
          p.map((x) => (x.id === localId ? { ...x, error: 'Upload failed', progress: 0 } : x)),
        );
      }
    }
  }

  function handleChange(e: ChangeEvent<HTMLInputElement>) {
    const files = Array.from(e.target.files ?? []);
    // Reset input so the same file can be chosen again; prior selections stay in attachments
    e.target.value = '';
    void uploadSequential(files);
  }

  async function remove(attachmentId: number) {
    await erApi.deleteAttachment(erId, attachmentId);
    onChange(attachments.filter((a) => a.id !== attachmentId));
  }

  return (
    <div className="uploader">
      {editable && (
        <div className="uploader-actions">
          <button type="button" className="btn" onClick={() => inputRef.current?.click()}>
            Upload files
          </button>
          <input ref={inputRef} type="file" multiple hidden onChange={handleChange} />
          <span className="hint">{attachments.length} file(s) attached</span>
        </div>
      )}

      <div className="attachment-grid">
        {attachments.map((a) => (
          <figure key={a.id} className="attachment-card">
            {a.isImage ? (
              <button type="button" className="thumb" onClick={() => setLightbox(a.url)}>
                <img src={a.url} alt={a.fileName} />
              </button>
            ) : (
              <div className="file-chip">{a.fileName}</div>
            )}
            <figcaption>
              <span title={a.fileName}>{a.fileName}</span>
              {editable && (
                <button type="button" className="linkish" onClick={() => void remove(a.id)}>
                  Remove
                </button>
              )}
            </figcaption>
          </figure>
        ))}
        {pending.map((p) => (
          <figure key={p.id} className="attachment-card pending">
            {p.previewUrl ? (
              <img src={p.previewUrl} alt={p.name} />
            ) : (
              <div className="file-chip">{p.name}</div>
            )}
            <figcaption>
              <span>{p.name}</span>
              {p.error ? <em className="error">{p.error}</em> : <em>{p.progress}%</em>}
            </figcaption>
          </figure>
        ))}
      </div>

      {lightbox && (
        <div className="lightbox" role="dialog" onClick={() => setLightbox(null)}>
          <img src={lightbox} alt="Preview" onClick={(e) => e.stopPropagation()} />
          <button type="button" className="btn ghost" onClick={() => setLightbox(null)}>
            Close
          </button>
        </div>
      )}
    </div>
  );
}
