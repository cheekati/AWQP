CREATE SCHEMA erp;
GO
CREATE SCHEMA auth;
GO
CREATE SCHEMA sales;
GO
CREATE SCHEMA mes;
GO
CREATE SCHEMA qms;
GO
CREATE SCHEMA whs;
GO
CREATE SCHEMA cleanroom;
GO
CREATE SCHEMA shipping;
GO
CREATE SCHEMA docs;
GO

CREATE TABLE erp.Customers (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
    CustomerCode NVARCHAR(32) NOT NULL CONSTRAINT UQ_Customers_Code UNIQUE,
    Name NVARCHAR(256) NOT NULL,
    TaxNumber NVARCHAR(64) NULL,
    Website NVARCHAR(256) NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Customers_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Customers_IsDeleted DEFAULT 0
);

CREATE TABLE erp.ProductCategories (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ProductCategories PRIMARY KEY,
    Name NVARCHAR(128) NOT NULL,
    IndustrySegment INT NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ProductCategories_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_ProductCategories_IsDeleted DEFAULT 0,
    CONSTRAINT UQ_ProductCategories_NameIndustry UNIQUE (Name, IndustrySegment)
);

CREATE TABLE erp.Products (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
    ProductCode NVARCHAR(64) NOT NULL CONSTRAINT UQ_Products_Code UNIQUE,
    Name NVARCHAR(256) NOT NULL,
    ProductCategoryId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Products_Category REFERENCES erp.ProductCategories(Id),
    MaterialGrade NVARCHAR(128) NOT NULL,
    OuterDiameterMm DECIMAL(18,4) NOT NULL,
    InnerDiameterMm DECIMAL(18,4) NOT NULL,
    LengthMm DECIMAL(18,4) NOT NULL,
    WallThicknessMm DECIMAL(18,4) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Products_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Products_IsDeleted DEFAULT 0
);

CREATE TABLE mes.Machines (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Machines PRIMARY KEY,
    MachineCode NVARCHAR(64) NOT NULL CONSTRAINT UQ_Machines_Code UNIQUE,
    Name NVARCHAR(128) NOT NULL,
    WorkCenter NVARCHAR(128) NOT NULL,
    IsCleanRoomQualified BIT NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Machines_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Machines_IsDeleted DEFAULT 0
);

CREATE TABLE mes.WorkOrders (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkOrders PRIMARY KEY,
    WorkOrderNumber NVARCHAR(64) NOT NULL CONSTRAINT UQ_WorkOrders_Number UNIQUE,
    BatchNumber NVARCHAR(64) NOT NULL,
    LotNumber NVARCHAR(64) NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_WorkOrders_Product REFERENCES erp.Products(Id),
    QuantityPlanned INT NOT NULL CONSTRAINT CK_WorkOrders_QtyPlan CHECK (QuantityPlanned > 0),
    QuantityProduced INT NOT NULL CONSTRAINT DF_WorkOrders_QtyProduced DEFAULT 0,
    QuantityRejected INT NOT NULL CONSTRAINT DF_WorkOrders_QtyRejected DEFAULT 0,
    Status INT NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_WorkOrders_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_WorkOrders_IsDeleted DEFAULT 0
);
CREATE INDEX IX_WorkOrders_BatchLot ON mes.WorkOrders(BatchNumber, LotNumber);

CREATE TABLE mes.ManufacturingOperations (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ManufacturingOperations PRIMARY KEY,
    WorkOrderId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Operations_WorkOrder REFERENCES mes.WorkOrders(Id),
    Stage INT NOT NULL,
    Sequence INT NOT NULL,
    OperatorName NVARCHAR(128) NOT NULL,
    MachineId UNIQUEIDENTIFIER NULL CONSTRAINT FK_Operations_Machine REFERENCES mes.Machines(Id),
    StartTimeUtc DATETIME2 NULL,
    EndTimeUtc DATETIME2 NULL,
    QuantityPlanned INT NOT NULL,
    QuantityProduced INT NOT NULL CONSTRAINT DF_Operations_Produced DEFAULT 0,
    QuantityRejected INT NOT NULL CONSTRAINT DF_Operations_Rejected DEFAULT 0,
    YieldPercentage AS CONVERT(DECIMAL(9,2), CASE WHEN QuantityProduced + QuantityRejected = 0 THEN 0 ELSE QuantityProduced * 100.0 / (QuantityProduced + QuantityRejected) END) PERSISTED,
    Status INT NOT NULL,
    Remarks NVARCHAR(1000) NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Operations_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Operations_IsDeleted DEFAULT 0,
    CONSTRAINT UQ_Operations_WorkOrderSequence UNIQUE (WorkOrderId, Sequence)
);

CREATE TABLE mes.ProductSerials (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ProductSerials PRIMARY KEY,
    SerialNumber NVARCHAR(96) NOT NULL CONSTRAINT UQ_ProductSerials_Serial UNIQUE,
    WorkOrderId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_ProductSerials_WorkOrder REFERENCES mes.WorkOrders(Id),
    ManufacturingDateUtc DATETIME2 NOT NULL,
    CurrentStatus NVARCHAR(64) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ProductSerials_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_ProductSerials_IsDeleted DEFAULT 0
);

CREATE TABLE qms.InspectionRecords (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_InspectionRecords PRIMARY KEY,
    InspectionNumber NVARCHAR(64) NOT NULL CONSTRAINT UQ_InspectionRecords_Number UNIQUE,
    WorkOrderId UNIQUEIDENTIFIER NULL CONSTRAINT FK_Inspection_WorkOrder REFERENCES mes.WorkOrders(Id),
    ProductSerialId UNIQUEIDENTIFIER NULL CONSTRAINT FK_Inspection_ProductSerial REFERENCES mes.ProductSerials(Id),
    InspectionStage INT NOT NULL,
    InspectorName NVARCHAR(128) NOT NULL,
    InspectionDateUtc DATETIME2 NOT NULL,
    Disposition INT NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Inspection_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Inspection_IsDeleted DEFAULT 0
);

CREATE TABLE cleanroom.CleanRooms (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CleanRooms PRIMARY KEY,
    CleanRoomCode NVARCHAR(32) NOT NULL CONSTRAINT UQ_CleanRooms_Code UNIQUE,
    Name NVARCHAR(128) NOT NULL,
    CleanRoomClass NVARCHAR(32) NOT NULL,
    TemperatureMinC DECIMAL(6,2) NOT NULL,
    TemperatureMaxC DECIMAL(6,2) NOT NULL,
    HumidityMinPercent DECIMAL(6,2) NOT NULL,
    HumidityMaxPercent DECIMAL(6,2) NOT NULL,
    ParticleCountMax INT NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_CleanRooms_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_CleanRooms_IsDeleted DEFAULT 0
);

CREATE TABLE cleanroom.EnvironmentalReadings (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EnvironmentalReadings PRIMARY KEY,
    CleanRoomId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Readings_CleanRoom REFERENCES cleanroom.CleanRooms(Id),
    ReadingTimeUtc DATETIME2 NOT NULL,
    TemperatureC DECIMAL(6,2) NOT NULL,
    HumidityPercent DECIMAL(6,2) NOT NULL,
    ParticleCount INT NOT NULL,
    ComplianceStatus INT NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Readings_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Readings_IsDeleted DEFAULT 0
);
CREATE INDEX IX_EnvironmentalReadings_RoomTime ON cleanroom.EnvironmentalReadings(CleanRoomId, ReadingTimeUtc DESC);
GO
