const apiBase = window.localStorage.getItem("awqpApiBase") || "";
const token = () => window.localStorage.getItem("awqpAccessToken") || "";

function authHeaders() {
    const value = token();
    return value ? { Authorization: `Bearer ${value}` } : {};
}

function getJson(url, done) {
    $.ajax({ url: apiBase + url, method: "GET", headers: authHeaders() })
        .done(done)
        .fail(() => console.warn(`Request failed: ${url}`));
}

function postJson(url, payload, done) {
    $.ajax({
        url: apiBase + url,
        method: "POST",
        headers: { ...authHeaders(), "Content-Type": "application/json" },
        data: JSON.stringify(payload)
    }).done(done);
}

function formToJson(form) {
    return Object.fromEntries(new FormData(form).entries());
}

function loadDashboards() {
    getJson("/api/dashboards/production", data => {
        $("#dailyProduction").text(data.dailyProduction ?? "--");
        $("#yieldPercent").text(`${data.yieldPercent ?? 0}%`);
    });
    getJson("/api/dashboards/quality", data => $("#ncrCount").text(data.ncrCount ?? "--"));
    getJson("/api/dashboards/clean-room", data => {
        $("#cleanRoomCompliance").text(data.complianceStatus ? "Compliant" : "Alert");
        $("#temperature").text(`${data.averageTemperatureC ?? 0} C`);
        $("#humidity").text(`${data.averageHumidityPercent ?? 0}%`);
        $("#particles").text(data.maxParticleCount ?? 0);
    });
    getJson("/api/dashboards/shipping", data => {
        $("#pendingDispatches").text(data.pendingDispatches ?? "--");
        $("#deliveriesToday").text(data.deliveriesToday ?? "--");
    });
}

function loadCustomers() {
    getJson("/api/customers", rows => {
        $("#customersTable").html((rows || []).map(x =>
            `<tr><td>${x.code}</td><td>${x.name}</td><td>${x.legalName}</td><td>${x.paymentTerms}</td><td>${x.creditStatus}</td></tr>`).join(""));
    });
}

function loadWorkOrders() {
    getJson("/api/work-orders/open", rows => {
        $("#workOrdersTable").html((rows || []).map(x =>
            `<tr><td>${x.workOrderNumber}</td><td>${x.batchNumber}</td><td>${x.lotNumber}</td><td>${x.quantityPlanned}</td><td>${x.quantityProduced}</td><td>${x.quantityRejected}</td><td>${x.yieldPercentage}%</td><td>${x.status}</td></tr>`).join(""));
    });
}

function loadQualityDashboard() {
    getJson("/api/dashboards/quality", data => {
        $("#qualityNcr").text(data.ncrCount ?? 0);
        $("#qualityCapa").text(data.openCapaCount ?? 0);
        $("#defectAnalysis").text(JSON.stringify(data.defectAnalysis || {}, null, 2));
    });
}

function loadInventoryDashboard() {
    getJson("/api/dashboards/inventory", data => {
        $("#currentStock").text(data.currentStock ?? 0);
        $("#inventoryValue").text(data.inventoryValuation ?? 0);
        $("#belowReorder").text(data.itemsBelowReorderLevel ?? 0);
    });
}

function loadShippingDashboard() {
    getJson("/api/dashboards/shipping", data => {
        $("#shippingPending").text(data.pendingDispatches ?? 0);
        $("#shippingDelivered").text(data.deliveriesToday ?? 0);
    });
}
