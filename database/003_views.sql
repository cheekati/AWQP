CREATE OR ALTER VIEW erp.vwProductionDashboard AS
SELECT
    CAST(SYSUTCDATETIME() AS DATE) AS DashboardDate,
    SUM(CASE WHEN wo.ManufacturingDate = CAST(SYSUTCDATETIME() AS DATE) THEN wo.QuantityProduced ELSE 0 END) AS DailyProduction,
    AVG(NULLIF(wo.YieldPercentage, 0)) AS YieldPercent,
    CAST(SUM(CASE WHEN mo.Status = 4 THEN 1 ELSE 0 END) * 100.0 / NULLIF(COUNT(mo.Id), 0) AS DECIMAL(18,2)) AS ProductionEfficiencyPercent,
    COUNT(DISTINCT mo.MachineId) AS MachinesUsed
FROM erp.WorkOrders wo
LEFT JOIN erp.ManufacturingOperations mo ON mo.WorkOrderId = wo.Id
WHERE wo.IsDeleted = 0;
GO

CREATE OR ALTER VIEW erp.vwQualityDashboard AS
SELECT
    (SELECT COUNT(*) FROM erp.NonConformanceReports WHERE IsDeleted = 0 AND Status <> 3) AS NcrCount,
    (SELECT COUNT(*) FROM erp.CorrectivePreventiveActions WHERE IsDeleted = 0 AND Status <> 6) AS OpenCapaCount,
    d.DefectCode,
    COUNT(d.Id) AS DefectCount
FROM erp.Defects d
WHERE d.IsDeleted = 0
GROUP BY d.DefectCode;
GO

CREATE OR ALTER VIEW erp.vwInventoryDashboard AS
SELECT
    SUM(QuantityOnHand) AS CurrentStock,
    SUM(QuantityOnHand * StandardCost) AS InventoryValuation,
    SUM(CASE WHEN QuantityOnHand <= ReorderLevel THEN 1 ELSE 0 END) AS ItemsBelowReorderLevel
FROM erp.InventoryItems
WHERE IsDeleted = 0;
GO

CREATE OR ALTER VIEW erp.vwCleanRoomDashboard AS
SELECT
    a.Code AS CleanRoomCode,
    a.Name AS CleanRoomName,
    AVG(r.TemperatureC) AS AverageTemperatureC,
    AVG(r.HumidityPercent) AS AverageHumidityPercent,
    MAX(r.ParticleCountPerCubicFoot) AS MaxParticleCount,
    CAST(MIN(CASE WHEN r.IsCompliant = 1 THEN 1 ELSE 0 END) AS BIT) AS ComplianceStatus
FROM erp.CleanRoomAreas a
LEFT JOIN erp.CleanRoomEnvironmentalReadings r
    ON r.CleanRoomAreaId = a.Id
    AND r.RecordedAt >= DATEADD(HOUR, -24, SYSUTCDATETIME())
WHERE a.IsDeleted = 0
GROUP BY a.Code, a.Name;
GO

CREATE OR ALTER VIEW erp.vwShippingDashboard AS
SELECT
    SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) AS PendingDispatches,
    SUM(CASE WHEN CAST(DeliveredAt AS DATE) = CAST(SYSUTCDATETIME() AS DATE) THEN 1 ELSE 0 END) AS DeliveriesToday,
    Status,
    COUNT(*) AS ShipmentCount
FROM erp.Shipments
WHERE IsDeleted = 0
GROUP BY Status;
GO

CREATE OR ALTER VIEW erp.vwProductGenealogy AS
SELECT
    ps.SerialNumber,
    ps.BatchNumber,
    ps.LotNumber,
    wo.WorkOrderNumber,
    ps.ManufacturingDate,
    mo.Stage,
    mo.OperatorUserId,
    m.Code AS MachineCode,
    ir.InspectionNumber,
    vp.PackagingBatchNumber,
    s.ShipmentNumber,
    te.EventType,
    te.EventReference,
    te.EventAt
FROM erp.ProductSerials ps
JOIN erp.WorkOrders wo ON wo.Id = ps.WorkOrderId
LEFT JOIN erp.TraceabilityEvents te ON te.ProductSerialId = ps.Id
LEFT JOIN erp.ManufacturingOperations mo ON mo.Id = te.ManufacturingOperationId
LEFT JOIN erp.Machines m ON m.Id = mo.MachineId
LEFT JOIN erp.InspectionRecords ir ON ir.Id = te.InspectionRecordId
LEFT JOIN erp.VacuumPackagingRecords vp ON vp.Id = te.VacuumPackagingRecordId
LEFT JOIN erp.Shipments s ON s.Id = te.ShipmentId
WHERE ps.IsDeleted = 0;
GO
