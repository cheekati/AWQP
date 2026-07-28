import { useEffect, useState } from 'react';
import api from '../api/client';

/** Loads protected attachment bytes with JWT and exposes a blob URL. */
export function useAuthFileUrl(downloadPath: string | null | undefined) {
  const [url, setUrl] = useState<string | null>(null);

  useEffect(() => {
    if (!downloadPath) {
      setUrl(null);
      return;
    }

    let objectUrl: string | null = null;
    let cancelled = false;

    const load = async () => {
      try {
        // downloadPath may be absolute API URL or relative /api/...
        const path = downloadPath.includes('/api/')
          ? downloadPath.substring(downloadPath.indexOf('/api/') + 4)
          : downloadPath;
        const res = await api.get(path, { responseType: 'blob' });
        objectUrl = URL.createObjectURL(res.data);
        if (!cancelled) setUrl(objectUrl);
      } catch {
        if (!cancelled) setUrl(null);
      }
    };

    void load();
    return () => {
      cancelled = true;
      if (objectUrl) URL.revokeObjectURL(objectUrl);
    };
  }, [downloadPath]);

  return url;
}

export function AuthImage({
  downloadPath,
  alt,
  className,
}: {
  downloadPath: string;
  alt: string;
  className?: string;
}) {
  const url = useAuthFileUrl(downloadPath);
  if (!url) return <div className="file-chip">Loading image...</div>;
  return <img src={url} alt={alt} className={className} />;
}
