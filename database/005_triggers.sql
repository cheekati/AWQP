CREATE OR ALTER TRIGGER erp.trg_CleanRoomReading_Compliance
ON erp.CleanRoomEnvironmentalReadings
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE r
    SET IsCompliant =
        CASE WHEN r.TemperatureC BETWEEN a.TemperatureLowerC AND a.TemperatureUpperC
              AND r.HumidityPercent BETWEEN a.HumidityLowerPercent AND a.HumidityUpperPercent
              AND r.ParticleCountPerCubicFoot <= a.ParticleLimitPerCubicFoot
             THEN 1 ELSE 0 END
    FROM erp.CleanRoomEnvironmentalReadings r
    JOIN inserted i ON i.Id = r.Id
    JOIN erp.CleanRoomAreas a ON a.Id = r.CleanRoomAreaId;
END;
GO

CREATE OR ALTER TRIGGER erp.trg_InventoryTransactions_AdjustStock
ON erp.InventoryTransactions
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE item
    SET QuantityOnHand =
        CASE
            WHEN i.TransactionType IN (1,4) THEN item.QuantityOnHand + i.Quantity
            WHEN i.TransactionType IN (2,3) THEN item.QuantityOnHand - i.Quantity
            WHEN i.TransactionType = 5 THEN i.Quantity
            ELSE item.QuantityOnHand
        END,
        WarehouseLocationId = COALESCE(i.ToLocationId, item.WarehouseLocationId),
        ModifiedAt = SYSUTCDATETIME(),
        ModifiedBy = i.CreatedBy
    FROM erp.InventoryItems item
    JOIN inserted i ON i.InventoryItemId = item.Id;
END;
GO

CREATE OR ALTER TRIGGER erp.trg_ManufacturingOperations_UpdateWorkOrderYield
ON erp.ManufacturingOperations
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE mo
    SET YieldPercentage = CASE
            WHEN mo.QuantityProduced + mo.QuantityRejected = 0 THEN 0
            ELSE ROUND(mo.QuantityProduced / (mo.QuantityProduced + mo.QuantityRejected) * 100, 2)
        END
    FROM erp.ManufacturingOperations mo
    JOIN inserted i ON i.Id = mo.Id;

    UPDATE wo
    SET QuantityProduced = agg.QuantityProduced,
        QuantityRejected = agg.QuantityRejected,
        YieldPercentage = CASE WHEN agg.QuantityProduced + agg.QuantityRejected = 0 THEN 0 ELSE ROUND(agg.QuantityProduced / (agg.QuantityProduced + agg.QuantityRejected) * 100, 2) END,
        ModifiedAt = SYSUTCDATETIME()
    FROM erp.WorkOrders wo
    JOIN (
        SELECT WorkOrderId, SUM(QuantityProduced) AS QuantityProduced, SUM(QuantityRejected) AS QuantityRejected
        FROM erp.ManufacturingOperations
        WHERE WorkOrderId IN (SELECT DISTINCT WorkOrderId FROM inserted)
        GROUP BY WorkOrderId
    ) agg ON agg.WorkOrderId = wo.Id;
END;
GO

CREATE OR ALTER TRIGGER erp.trg_ProductSerial_CreateTraceEvent
ON erp.ProductSerials
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO erp.TraceabilityEvents
        (Id, ProductSerialId, EventType, EventReference, EventAt, CreatedBy)
    SELECT NEWID(), Id, 'SerialCreated', SerialNumber, SYSUTCDATETIME(), CreatedBy
    FROM inserted;
END;
GO

CREATE OR ALTER TRIGGER erp.trg_Audit_Customers
ON erp.Customers
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO erp.AuditLogs (Id, EntityName, EntityId, Action, ChangedBy, ChangedAt, OldValuesJson, NewValuesJson)
    SELECT
        NEWID(),
        'Customer',
        COALESCE(CONVERT(NVARCHAR(80), i.Id), CONVERT(NVARCHAR(80), d.Id)),
        CASE WHEN i.Id IS NOT NULL AND d.Id IS NULL THEN 'Insert'
             WHEN i.Id IS NOT NULL AND d.Id IS NOT NULL THEN 'Update'
             ELSE 'Delete' END,
        COALESCE(i.ModifiedBy, i.CreatedBy, d.ModifiedBy, d.CreatedBy, 'system'),
        SYSUTCDATETIME(),
        (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
        (SELECT i.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i
    FULL OUTER JOIN deleted d ON d.Id = i.Id;
END;
GO
