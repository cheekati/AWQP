// Shared helpers for the MARUWA EHS / PPE screens.
// window.PPE_CONFIG = { apiBase, token } is injected by each Razor view.
(function () {
    const cfg = window.PPE_CONFIG || { apiBase: 'http://localhost:5080', token: '' };
    const apiBase = (cfg.apiBase || '').replace(/\/+$/, '');

    function api(path, method, body) {
        return $.ajax({
            url: apiBase + path,
            method: method || 'GET',
            contentType: 'application/json',
            headers: cfg.token ? { Authorization: 'Bearer ' + cfg.token } : {},
            data: body ? JSON.stringify(body) : undefined
        });
    }

    function errorText(xhr) {
        try {
            const j = JSON.parse(xhr.responseText);
            if (j.error) return j.error;
            if (j.errors) return Object.values(j.errors).flat().join(' ');
            if (j.title) return j.title;
        } catch (e) { /* ignore */ }
        return 'Request failed (' + xhr.status + ').';
    }

    function money(v) {
        return Number(v || 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function escapeHtml(s) {
        return String(s == null ? '' : s).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    }

    // Minimal canvas signature pad (mouse + touch).
    function signaturePad(canvas) {
        const ctx = canvas.getContext('2d');
        let drawing = false, dirty = false;
        ctx.lineWidth = 2; ctx.lineCap = 'round'; ctx.strokeStyle = '#0b2e6b';

        function pos(e) {
            const r = canvas.getBoundingClientRect();
            const t = e.touches ? e.touches[0] : e;
            return { x: t.clientX - r.left, y: t.clientY - r.top };
        }
        function start(e) { drawing = true; dirty = true; const p = pos(e); ctx.beginPath(); ctx.moveTo(p.x, p.y); e.preventDefault(); }
        function move(e) { if (!drawing) return; const p = pos(e); ctx.lineTo(p.x, p.y); ctx.stroke(); e.preventDefault(); }
        function end() { drawing = false; }

        canvas.addEventListener('mousedown', start);
        canvas.addEventListener('mousemove', move);
        window.addEventListener('mouseup', end);
        canvas.addEventListener('touchstart', start);
        canvas.addEventListener('touchmove', move);
        canvas.addEventListener('touchend', end);

        return {
            clear() { ctx.clearRect(0, 0, canvas.width, canvas.height); dirty = false; },
            isEmpty() { return !dirty; },
            toDataUrl() { return dirty ? canvas.toDataURL('image/png') : ''; }
        };
    }

    window.PPE = { api, errorText, money, escapeHtml, signaturePad };
})();
