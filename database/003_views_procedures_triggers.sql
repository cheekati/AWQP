CREATE OR ALTER VIEW mes.vw_WorkOrderTraceability
AS
SELECT ps.SerialNumber, wo.WorkOrderNumber, wo.BatchNumber, wo.LotNumber, p.ProductCode, p.Name AS ProductName,
       ps.ManufacturingDateUtc, wo.Status AS WorkOrderStatus
FROM mes.ProductSerials ps
JOIN mes.WorkOrders wo ON wo.Id = ps.WorkOrderId
JOIN erp.Products p ON p.Id = wo.ProductId
WHERE ps.IsDeleted = 0 AND wo.IsDeleted = 0;
GO

CREATE OR ALTER VIEW cleanroom.vw_CleanRoomComplianceLatest
AS
WITH ranked AS (
    SELECT cr.CleanRoomCode, cr.Name, er.ReadingTimeUtc, er.TemperatureC, er.HumidityPercent, er.ParticleCount, er.ComplianceStatus,
           ROW_NUMBER() OVER (PARTITION BY cr.Id ORDER BY er.ReadingTimeUtc DESC) rn
    FROM cleanroom.CleanRooms cr
    JOIN cleanroom.EnvironmentalReadings er ON er.CleanRoomId = cr.Id
    WHERE cr.IsDeleted = 0 AND er.IsDeleted = 0
)
SELECT CleanRoomCode, Name, ReadingTimeUtc, TemperatureC, HumidityPercent, ParticleCount, ComplianceStatus
FROM ranked WHERE rn = 1;
GO

CREATE OR ALTER PROCEDURE mes.usp_GetProductGenealogy
    @SerialNumber NVARCHAR(96)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM mes.vw_WorkOrderTraceability WHERE SerialNumber = @SerialNumber;

    SELECT mo.Stage, mo.Sequence, mo.OperatorName, m.MachineCode, mo.StartTimeUtc, mo.EndTimeUtc,
           mo.QuantityPlanned, mo.QuantityProduced, mo.QuantityRejected, mo.YieldPercentage, mo.Status, mo.Remarks
    FROM mes.ProductSerials ps
    JOIN mes.WorkOrders wo ON wo.Id = ps.WorkOrderId
    JOIN mes.ManufacturingOperations mo ON mo.WorkOrderId = wo.Id
    LEFT JOIN mes.Machines m ON m.Id = mo.MachineId
    WHERE ps.SerialNumber = @SerialNumber
    ORDER BY mo.Sequence;

    SELECT ir.InspectionNumber, ir.InspectionStage, ir.InspectorName, ir.InspectionDateUtc, ir.Disposition
    FROM mes.ProductSerials ps
    JOIN qms.InspectionRecords ir ON ir.ProductSerialId = ps.Id OR ir.WorkOrderId = ps.WorkOrderId
    WHERE ps.SerialNumber = @SerialNumber;
END;
GO

CREATE OR ALTER TRIGGER cleanroom.trg_EnvironmentalReadings_AuditException
ON cleanroom.EnvironmentalReadings
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO cleanroom.EnvironmentalReadings (Id, CleanRoomId, ReadingTimeUtc, TemperatureC, HumidityPercent, ParticleCount, ComplianceStatus, CreatedBy)
    SELECT NEWID(), i.CleanRoomId, SYSUTCDATETIME(), i.TemperatureC, i.HumidityPercent, i.ParticleCount, 4, 'trigger-alert'
    FROM inserted i
    WHERE i.ComplianceStatus = 3 AND NOT EXISTS (
        SELECT 1 FROM cleanroom.EnvironmentalReadings e
        WHERE e.CleanRoomId = i.CleanRoomId AND e.CreatedBy = 'trigger-alert' AND e.ReadingTimeUtc >= DATEADD(MINUTE, -5, SYSUTCDATETIME())
    );
END;
GO
