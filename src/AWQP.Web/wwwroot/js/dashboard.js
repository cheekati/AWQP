(function () {
    const apiBase = window.localStorage.getItem('awqp.apiBase') || 'https://localhost:7001';
    function setText(id, value) { const el = document.getElementById(id); if (el) el.textContent = value; }
    $.ajax({ url: apiBase + '/api/dashboards/executive', method: 'GET' })
        .done(function (data) {
            setText('metric-production', data.production?.[0]?.value ?? '--');
            setText('metric-yield', data.production?.[1]?.value ?? '--');
            setText('metric-cleanroom', data.cleanRoom?.[0]?.value ?? '--');
            setText('metric-shipping', data.shipping?.[0]?.value ?? '--');
        })
        .fail(function () {
            $('.metric-card strong').text('demo');
        });
})();
