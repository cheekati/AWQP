// Work Permit Supplier Progress — front page with daily atmospheric data + acceptable ranges.
(function () {
    const cfg = window.WP_CONFIG || { apiBase: '/progressworkorderSupplier', token: '', supplierCode: 'SUP001' };
    const apiBase = (cfg.apiBase || '').replace(/\/+$/, '');
    let ranges = [];
    let permits = [];
    let editModal;

    function api(path, method, body) {
        return $.ajax({
            url: apiBase + path,
            method: method || 'GET',
            contentType: 'application/json',
            headers: cfg.token ? { Authorization: 'Bearer ' + cfg.token } : {},
            data: body ? JSON.stringify(body) : undefined
        });
    }

    function escapeHtml(s) {
        return String(s == null ? '' : s).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    }

    function errorText(xhr) {
        try {
            const j = JSON.parse(xhr.responseText);
            if (j.error) return j.error;
            if (j.title) return j.title;
        } catch (e) { /* ignore */ }
        return 'Request failed (' + (xhr && xhr.status) + ').';
    }

    function alertMsg(msg, ok) {
        $('#alert-box').html(
            '<div class="alert alert-' + (ok ? 'success' : 'danger') + ' alert-dismissible fade show" role="alert">' +
            escapeHtml(msg) +
            '<button type="button" class="btn-close" data-bs-dismiss="alert"></button></div>'
        );
    }

    function rangeLookup(code) {
        return ranges.find(r => (r.parameterCode || r.ParameterCode) === code) || {};
    }

    function displayRange(code) {
        const r = rangeLookup(code);
        return r.displayRange || r.DisplayRange || '';
    }

    function statusBadge(status) {
        const s = (status || '').toUpperCase();
        const map = {
            'REQUEST': 'text-bg-info',
            'APPROVED': 'text-bg-success',
            'IN PROGRESS': 'wp-status-inpro',
            'ON HOLD': 'wp-status-onhold',
            'COMPLETED': 'wp-status-comp',
            'VERIFIED': 'wp-status-veri',
            'REJECTED': 'text-bg-danger'
        };
        return '<span class="badge ' + (map[s] || 'text-bg-secondary') + '">' + escapeHtml(status) + '</span>';
    }

    function fmtDate(d) {
        if (!d) return '—';
        const dt = new Date(d);
        if (isNaN(dt.getTime())) return escapeHtml(String(d));
        return dt.toLocaleString(undefined, { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' });
    }

    function fmtDay(d) {
        if (!d) return '—';
        // DateOnly serializes as "yyyy-MM-dd"
        if (/^\d{4}-\d{2}-\d{2}/.test(d)) {
            const parts = d.substring(0, 10).split('-');
            const dt = new Date(Number(parts[0]), Number(parts[1]) - 1, Number(parts[2]));
            return dt.toLocaleDateString(undefined, { day: '2-digit', month: 'short', year: 'numeric' });
        }
        return fmtDate(d);
    }

    function cellClass(ok) {
        return ok ? 'atm-ok' : 'atm-bad';
    }

    function renderRanges() {
        if (!ranges.length) {
            $('#range-cards').html('<div class="col-12 text-muted">No acceptable ranges configured.</div>');
            return;
        }
        $('#range-cards').html(ranges.map(function (r) {
            const code = r.parameterCode || r.ParameterCode;
            const name = r.parameterName || r.ParameterName;
            const unit = r.unit || r.Unit;
            const display = r.displayRange || r.DisplayRange;
            const min = r.minAcceptable ?? r.MinAcceptable;
            const max = r.maxAcceptable ?? r.MaxAcceptable;
            return '<div class="col-md-3 col-sm-6">' +
                '<div class="range-card h-100">' +
                '<div class="range-code">' + escapeHtml(code) + '</div>' +
                '<div class="range-name">' + escapeHtml(name) + '</div>' +
                '<div class="range-value">' + escapeHtml(display) + '</div>' +
                '<div class="range-meta text-muted small">Unit: ' + escapeHtml(unit) +
                (min != null ? ' · Min: ' + min : '') +
                (max != null ? ' · Max: ' + max : '') +
                '</div></div></div>';
        }).join(''));
    }

    function dailyTable(readings) {
        const list = readings || [];
        if (!list.length) {
            return '<div class="text-muted small py-1">No daily atmospheric readings yet.</div>';
        }
        const o2r = displayRange('O2');
        const h2sr = displayRange('H2S');
        const cor = displayRange('CO');
        const lelr = displayRange('LEL');
        let html = '<table class="table table-sm table-bordered mb-0 atm-daily">' +
            '<thead><tr>' +
            '<th>Date</th>' +
            '<th>O₂ % <span class="range-hint">(' + escapeHtml(o2r) + ')</span></th>' +
            '<th>H₂S ppm <span class="range-hint">(' + escapeHtml(h2sr) + ')</span></th>' +
            '<th>CO ppm <span class="range-hint">(' + escapeHtml(cor) + ')</span></th>' +
            '<th>Combustible %LEL <span class="range-hint">(' + escapeHtml(lelr) + ')</span></th>' +
            '<th>PIC</th>' +
            '<th>Result</th>' +
            '</tr></thead><tbody>';
        list.forEach(function (r) {
            const o2Ok = r.oxygenInRange ?? r.OxygenInRange;
            const h2sOk = r.h2SInRange ?? r.H2SInRange;
            const coOk = r.coInRange ?? r.CoInRange;
            const lelOk = r.combustibleInRange ?? r.CombustibleInRange;
            const allOk = r.allInRange ?? r.AllInRange;
            html += '<tr>' +
                '<td>' + fmtDay(r.readingDate || r.ReadingDate) + '</td>' +
                '<td class="' + cellClass(o2Ok) + '">' + (r.oxygenContentPercent ?? r.OxygenContentPercent) + '</td>' +
                '<td class="' + cellClass(h2sOk) + '">' + (r.toxicGasH2SPpm ?? r.ToxicGasH2SPpm) + '</td>' +
                '<td class="' + cellClass(coOk) + '">' + (r.carbonMonoxidePpm ?? r.CarbonMonoxidePpm) + '</td>' +
                '<td class="' + cellClass(lelOk) + '">' + (r.combustibleGasLelPercent ?? r.CombustibleGasLelPercent) + '</td>' +
                '<td>' + escapeHtml(r.picName || r.PicName || '') + '</td>' +
                '<td>' + (allOk
                    ? '<span class="badge text-bg-success">In Range</span>'
                    : '<span class="badge text-bg-danger">Out of Range</span>') + '</td>' +
                '</tr>';
        });
        html += '</tbody></table>';
        return html;
    }

    function renderGrid(filter) {
        const q = (filter || '').toLowerCase().trim();
        const rows = permits.filter(function (p) {
            if (!q) return true;
            const hay = [
                p.permitNumber, p.status, p.workType, p.department, p.departmentName,
                p.section, p.sectionName, p.plantLocation, p.workInfo, p.poNumber, p.invoiceNumber
            ].join(' ').toLowerCase();
            return hay.indexOf(q) >= 0;
        });

        if (!rows.length) {
            $('#wp-grid').html('<tr><td colspan="11" class="text-center text-muted py-4">No in-progress work permits for this supplier.</td></tr>');
            return;
        }

        let html = '';
        rows.forEach(function (p, idx) {
            const readings = p.dailyAtmosphericReadings || p.DailyAtmosphericReadings || [];
            const detailId = 'atm-' + idx;
            const poLabel = p.noPoPr ? 'NO PO/PR' : (p.po ? 'PO' : (p.pr ? 'PR' : '—'));
            html += '<tr class="wp-main-row" data-id="' + p.id + '">' +
                '<td><button class="btn btn-sm btn-outline-secondary btn-toggle" data-target="' + detailId + '" title="Show daily data">▼</button></td>' +
                '<td><strong>' + escapeHtml(p.permitNumber) + '</strong></td>' +
                '<td>' + statusBadge(p.status) + '</td>' +
                '<td>' + escapeHtml(p.workType) + '</td>' +
                '<td>' + escapeHtml(p.department) + ' — ' + escapeHtml(p.departmentName) + '</td>' +
                '<td>' + escapeHtml(p.plantLocation) + '</td>' +
                '<td>' + escapeHtml(p.workInfo) + '</td>' +
                '<td class="small">' + fmtDate(p.workScheduleDateFrom) + '<br/>→ ' + fmtDate(p.workScheduleDateTo) + '</td>' +
                '<td class="small">' + escapeHtml(poLabel) + '<br/>' + escapeHtml(p.poNumber || '') +
                (p.poCost != null ? '<br/>' + Number(p.poCost).toFixed(2) : '') + '</td>' +
                '<td><span class="badge text-bg-secondary">' + readings.length + ' day(s)</span></td>' +
                '<td class="text-end"><button class="btn btn-sm btn-outline-primary btn-edit" data-id="' + p.id + '">Edit</button></td>' +
                '</tr>';
            html += '<tr id="' + detailId + '" class="wp-detail-row">' +
                '<td colspan="11" class="wp-detail-cell">' +
                '<div class="fw-semibold mb-2">Daily Atmospheric Testing Data <span class="text-muted fw-normal">(acceptable range shown per parameter)</span></div>' +
                dailyTable(readings) +
                '</td></tr>';
        });
        $('#wp-grid').html(html);
    }

    function load() {
        const supplier = ($('#supplier-code').val() || cfg.supplierCode || 'SUP001').trim();
        cfg.supplierCode = supplier;
        $('#wp-grid').html('<tr><td colspan="11" class="text-center text-muted py-4">Loading…</td></tr>');
        api('/api/ehs/work-permits/progress/' + encodeURIComponent(supplier))
            .done(function (data) {
                ranges = data.acceptableRanges || data.AcceptableRanges || [];
                permits = data.workPermits || data.WorkPermits || [];
                renderRanges();
                renderGrid($('#filter-text').val());
            })
            .fail(function (xhr) {
                alertMsg(errorText(xhr), false);
                $('#wp-grid').html('<tr><td colspan="11" class="text-center text-danger py-4">Failed to load work permits.</td></tr>');
            });
    }

    function openEdit(id) {
        const p = permits.find(x => x.id === id);
        if (!p) return;
        $('#f-id').val(p.id);
        let status = (p.status || '').toUpperCase();
        if (status === 'APPROVED') status = 'IN PROGRESS';
        $('#f-status').val(status);
        $('#f-invoice').val(p.invoiceNumber || '');
        $('#f-po').val(p.poNumber || '');
        $('#f-cost').val(p.poCost != null ? p.poCost : '');
        $('#f-image').val(p.supportingDocument6 || '');
        $('#f-upload').val('');
        $('#f-inprog').val(p.inProgressRemarks || '');
        $('#f-hold').val(p.onHoldRemarks || '');
        $('#f-comp').val(p.completedRemarks || '');
        $('#form-error').text('');
        const today = new Date();
        $('#r-date').val(today.toISOString().substring(0, 10));
        $('#r-o2').val('20.9'); $('#r-h2s').val('0'); $('#r-co').val('0'); $('#r-lel').val('0');
        $('#r-pic').val(''); $('#r-remarks').val('');
        editModal.show();
    }

    $(function () {
        editModal = new bootstrap.Modal(document.getElementById('edit-modal'));
        $('#btn-reload').on('click', load);
        $('#filter-text').on('input', function () { renderGrid(this.value); });
        $('#wp-grid').on('click', '.btn-toggle', function () {
            const id = $(this).data('target');
            $('#' + id).toggleClass('d-none');
        });
        $('#wp-grid').on('click', '.btn-edit', function () {
            openEdit($(this).data('id'));
        });
        $('#btn-save').on('click', function () {
            const id = $('#f-id').val();
            const body = {
                status: $('#f-status').val(),
                invoiceNumber: $('#f-invoice').val(),
                poNumber: $('#f-po').val(),
                poCost: $('#f-cost').val() === '' ? null : parseFloat($('#f-cost').val()),
                inProgressRemarks: $('#f-inprog').val(),
                onHoldRemarks: $('#f-hold').val(),
                completedRemarks: $('#f-comp').val(),
                supportingDocument6: $('#f-image').val() || null,
                uploadDocument1: $('#f-upload').val() || null
            };
            api('/api/ehs/work-permits/' + id + '/progress', 'PUT', body)
                .done(function () {
                    editModal.hide();
                    alertMsg('Work permit successfully updated.', true);
                    load();
                })
                .fail(function (xhr) { $('#form-error').text(errorText(xhr)); });
        });
        $('#btn-add-reading').on('click', function () {
            const id = $('#f-id').val();
            const body = {
                readingDate: $('#r-date').val(),
                oxygenContentPercent: parseFloat($('#r-o2').val()),
                toxicGasH2SPpm: parseFloat($('#r-h2s').val()),
                carbonMonoxidePpm: parseFloat($('#r-co').val()),
                combustibleGasLelPercent: parseFloat($('#r-lel').val()),
                picName: $('#r-pic').val() || null,
                remarks: $('#r-remarks').val() || null
            };
            if (!body.readingDate) {
                $('#form-error').text('Please enter reading date.');
                return;
            }
            api('/api/ehs/work-permits/' + id + '/daily-readings', 'POST', body)
                .done(function () {
                    alertMsg('Daily atmospheric reading added.', true);
                    load();
                    setTimeout(function () { openEdit(id); }, 400);
                })
                .fail(function (xhr) { $('#form-error').text(errorText(xhr)); });
        });
        load();
    });
})();
