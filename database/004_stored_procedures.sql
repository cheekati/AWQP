CREATE OR ALTER PROCEDURE erp.usp_GetProductGenealogy
    @SerialNumber NVARCHAR(120)
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Genealogy AS (
        SELECT ps.Id, ps.SerialNumber, ps.BatchNumber, ps.LotNumber, ps.CurrentStatus, CAST(0 AS INT) AS Level
        FROM erp.ProductSerials ps
        WHERE ps.SerialNumber = @SerialNumber

        UNION ALL

        SELECT child.Id, child.SerialNumber, child.BatchNumber, child.LotNumber, child.CurrentStatus, g.Level + 1
        FROM Genealogy g
        JOIN erp.ProductGenealogyLinks link ON link.ParentProductSerialId = g.Id
        JOIN erp.ProductSerials child ON child.Id = link.ChildProductSerialId
    )
    SELECT * FROM Genealogy ORDER BY Level, SerialNumber;

    SELECT * FROM erp.vwProductGenealogy WHERE SerialNumber = @SerialNumber ORDER BY EventAt;
END;
GO

CREATE OR ALTER PROCEDURE erp.usp_RecordInventoryTransaction
    @TransactionNumber NVARCHAR(80),
    @InventoryItemId UNIQUEIDENTIFIER,
    @TransactionType INT,
    @Quantity DECIMAL(18,4),
    @FromLocationId UNIQUEIDENTIFIER = NULL,
    @ToLocationId UNIQUEIDENTIFIER = NULL,
    @ReferenceNumber NVARCHAR(120) = NULL,
    @User NVARCHAR(128) = 'system'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;

    INSERT INTO erp.InventoryTransactions
        (Id, TransactionNumber, InventoryItemId, TransactionType, Quantity, FromLocationId, ToLocationId, ReferenceNumber, TransactionAt, CreatedBy)
    VALUES
        (NEWID(), @TransactionNumber, @InventoryItemId, @TransactionType, @Quantity, @FromLocationId, @ToLocationId, @ReferenceNumber, SYSUTCDATETIME(), @User);

    UPDATE erp.InventoryItems
    SET QuantityOnHand =
        CASE
            WHEN @TransactionType IN (1,4) THEN QuantityOnHand + @Quantity
            WHEN @TransactionType IN (2,3) THEN QuantityOnHand - @Quantity
            WHEN @TransactionType = 5 THEN @Quantity
            ELSE QuantityOnHand
        END,
        WarehouseLocationId = COALESCE(@ToLocationId, WarehouseLocationId),
        ModifiedAt = SYSUTCDATETIME(),
        ModifiedBy = @User
    WHERE Id = @InventoryItemId;

    COMMIT TRANSACTION;
END;
GO

CREATE OR ALTER PROCEDURE erp.usp_CompleteManufacturingOperation
    @OperationId UNIQUEIDENTIFIER,
    @QuantityProduced DECIMAL(18,4),
    @QuantityRejected DECIMAL(18,4),
    @Remarks NVARCHAR(1000) = NULL,
    @User NVARCHAR(128) = 'system'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;

    UPDATE erp.ManufacturingOperations
    SET EndTime = SYSUTCDATETIME(),
        QuantityProduced = @QuantityProduced,
        QuantityRejected = @QuantityRejected,
        YieldPercentage = CASE WHEN @QuantityProduced + @QuantityRejected = 0 THEN 0 ELSE ROUND(@QuantityProduced / (@QuantityProduced + @QuantityRejected) * 100, 2) END,
        Status = 4,
        Remarks = @Remarks,
        ModifiedAt = SYSUTCDATETIME(),
        ModifiedBy = @User
    WHERE Id = @OperationId;

    DECLARE @WorkOrderId UNIQUEIDENTIFIER = (SELECT WorkOrderId FROM erp.ManufacturingOperations WHERE Id = @OperationId);

    UPDATE wo
    SET QuantityProduced = agg.QuantityProduced,
        QuantityRejected = agg.QuantityRejected,
        YieldPercentage = CASE WHEN agg.QuantityProduced + agg.QuantityRejected = 0 THEN 0 ELSE ROUND(agg.QuantityProduced / (agg.QuantityProduced + agg.QuantityRejected) * 100, 2) END,
        Status = CASE WHEN agg.CompletedCount = agg.TotalCount THEN 5 ELSE 3 END,
        ModifiedAt = SYSUTCDATETIME(),
        ModifiedBy = @User
    FROM erp.WorkOrders wo
    CROSS APPLY (
        SELECT SUM(QuantityProduced) AS QuantityProduced,
               SUM(QuantityRejected) AS QuantityRejected,
               SUM(CASE WHEN Status = 4 THEN 1 ELSE 0 END) AS CompletedCount,
               COUNT(*) AS TotalCount
        FROM erp.ManufacturingOperations
        WHERE WorkOrderId = @WorkOrderId
    ) agg
    WHERE wo.Id = @WorkOrderId;

    COMMIT TRANSACTION;
END;
GO

CREATE OR ALTER PROCEDURE erp.usp_RecordCleanRoomReading
    @CleanRoomAreaId UNIQUEIDENTIFIER,
    @TemperatureC DECIMAL(18,4),
    @HumidityPercent DECIMAL(18,4),
    @ParticleCountPerCubicFoot INT,
    @SensorId NVARCHAR(80),
    @User NVARCHAR(128) = 'system'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IsCompliant BIT = 0;
    SELECT @IsCompliant =
        CASE WHEN @TemperatureC BETWEEN TemperatureLowerC AND TemperatureUpperC
              AND @HumidityPercent BETWEEN HumidityLowerPercent AND HumidityUpperPercent
              AND @ParticleCountPerCubicFoot <= ParticleLimitPerCubicFoot
             THEN 1 ELSE 0 END
    FROM erp.CleanRoomAreas
    WHERE Id = @CleanRoomAreaId;

    INSERT INTO erp.CleanRoomEnvironmentalReadings
        (Id, CleanRoomAreaId, RecordedAt, TemperatureC, HumidityPercent, ParticleCountPerCubicFoot, SensorId, IsCompliant, CreatedBy)
    VALUES
        (NEWID(), @CleanRoomAreaId, SYSUTCDATETIME(), @TemperatureC, @HumidityPercent, @ParticleCountPerCubicFoot, @SensorId, @IsCompliant, @User);
END;
GO
