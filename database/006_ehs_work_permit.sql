-- MARUWA EHS System - Work Permit (Supplier Progress) + daily atmospheric testing
SET QUOTED_IDENTIFIER ON;
GO

IF SCHEMA_ID('ehs') IS NULL EXEC('CREATE SCHEMA ehs');
GO

IF OBJECT_ID('ehs.AtmosphericParameterRanges') IS NULL
CREATE TABLE ehs.AtmosphericParameterRanges (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AtmosphericParameterRanges PRIMARY KEY,
    ParameterCode NVARCHAR(32) NOT NULL CONSTRAINT UQ_AtmosphericParameterRanges_Code UNIQUE,
    ParameterName NVARCHAR(128) NOT NULL,
    Unit NVARCHAR(32) NOT NULL,
    MinAcceptable DECIMAL(10,2) NULL,
    MaxAcceptable DECIMAL(10,2) NULL,
    DisplayRange NVARCHAR(64) NOT NULL,
    SortOrder INT NOT NULL CONSTRAINT DF_AtmosphericParameterRanges_Sort DEFAULT 0,
    IsActive BIT NOT NULL CONSTRAINT DF_AtmosphericParameterRanges_IsActive DEFAULT 1,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_AtmosphericParameterRanges_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_AtmosphericParameterRanges_IsDeleted DEFAULT 0
);
GO

IF OBJECT_ID('ehs.WorkPermits') IS NULL
CREATE TABLE ehs.WorkPermits (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkPermits PRIMARY KEY,
    PermitNumber BIGINT NOT NULL CONSTRAINT UQ_WorkPermits_Number UNIQUE,
    WorkType NVARCHAR(32) NOT NULL,
    Status NVARCHAR(32) NOT NULL,
    Department NVARCHAR(32) NOT NULL,
    DepartmentName NVARCHAR(128) NOT NULL,
    Section NVARCHAR(32) NOT NULL,
    SectionName NVARCHAR(128) NOT NULL,
    PlantLocation NVARCHAR(128) NOT NULL,
    WorkInfo NVARCHAR(256) NOT NULL,
    DetailDescription NVARCHAR(MAX) NULL,
    WorkScheduleDateFrom DATETIME2 NULL,
    WorkScheduleDateTo DATETIME2 NULL,
    SupplierCode NVARCHAR(32) NOT NULL,
    SupplierName NVARCHAR(128) NOT NULL,
    SupplierPic NVARCHAR(128) NULL,
    SupContactNo NVARCHAR(32) NULL,
    PoNumber NVARCHAR(64) NULL,
    PoCost DECIMAL(18,2) NULL,
    InvoiceNumber NVARCHAR(100) NULL,
    NoPoPr BIT NOT NULL CONSTRAINT DF_WorkPermits_NoPoPr DEFAULT 0,
    Po BIT NOT NULL CONSTRAINT DF_WorkPermits_Po DEFAULT 0,
    Pr BIT NOT NULL CONSTRAINT DF_WorkPermits_Pr DEFAULT 0,
    InProgressRemarks NVARCHAR(MAX) NULL,
    OnHoldRemarks NVARCHAR(MAX) NULL,
    CompletedRemarks NVARCHAR(MAX) NULL,
    InProgressBy NVARCHAR(128) NULL,
    InProgressOn DATETIME2 NULL,
    OnHoldBy NVARCHAR(128) NULL,
    OnHoldOn DATETIME2 NULL,
    CompletedBy NVARCHAR(128) NULL,
    CompletedOn DATETIME2 NULL,
    SupportingDocument6 NVARCHAR(512) NULL,
    UploadDocument1 NVARCHAR(512) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_WorkPermits_IsActive DEFAULT 1,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_WorkPermits_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_WorkPermits_IsDeleted DEFAULT 0
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WorkPermits_SupplierCode')
    CREATE INDEX IX_WorkPermits_SupplierCode ON ehs.WorkPermits(SupplierCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WorkPermits_Status')
    CREATE INDEX IX_WorkPermits_Status ON ehs.WorkPermits(Status);
GO

IF OBJECT_ID('ehs.DailyAtmosphericReadings') IS NULL
CREATE TABLE ehs.DailyAtmosphericReadings (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DailyAtmosphericReadings PRIMARY KEY,
    WorkPermitId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_DailyAtmosphericReadings_WorkPermit REFERENCES ehs.WorkPermits(Id),
    ReadingDate DATE NOT NULL,
    ReadingDateTimeUtc DATETIME2 NOT NULL,
    OxygenContentPercent DECIMAL(5,2) NOT NULL,
    ToxicGasH2SPpm DECIMAL(8,2) NOT NULL,
    CarbonMonoxidePpm DECIMAL(8,2) NOT NULL,
    CombustibleGasLelPercent DECIMAL(5,2) NOT NULL,
    PicName NVARCHAR(128) NULL,
    Remarks NVARCHAR(512) NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_DailyAtmosphericReadings_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_DailyAtmosphericReadings_IsDeleted DEFAULT 0,
    CONSTRAINT UQ_DailyAtmosphericReadings_PermitDate UNIQUE (WorkPermitId, ReadingDate)
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DailyAtmosphericReadings_WorkPermitId')
    CREATE INDEX IX_DailyAtmosphericReadings_WorkPermitId ON ehs.DailyAtmosphericReadings(WorkPermitId);
GO

-- Acceptable ranges for atmospheric testing parameters (front-page legend)
IF NOT EXISTS (SELECT 1 FROM ehs.AtmosphericParameterRanges WHERE ParameterCode = 'O2')
INSERT INTO ehs.AtmosphericParameterRanges (Id, ParameterCode, ParameterName, Unit, MinAcceptable, MaxAcceptable, DisplayRange, SortOrder, IsActive, CreatedAtUtc, CreatedBy, IsDeleted)
VALUES
(NEWID(), 'O2',  N'Oxygen Content',     N'%',   19.50, 23.50, N'19.5 – 23.5%', 1, 1, SYSUTCDATETIME(), N'seed', 0),
(NEWID(), 'H2S', N'Toxic Gas H2S',      N'ppm', 0.00,  10.00, N'≤ 10 ppm',     2, 1, SYSUTCDATETIME(), N'seed', 0),
(NEWID(), 'CO',  N'Carbon Monoxide',    N'ppm', 0.00,  25.00, N'≤ 25 ppm',     3, 1, SYSUTCDATETIME(), N'seed', 0),
(NEWID(), 'LEL', N'Combustible Gas',    N'%LEL',0.00,  10.00, N'≤ 10% LEL',    4, 1, SYSUTCDATETIME(), N'seed', 0);
GO

-- Sample supplier progress work permits with multi-day atmospheric readings
DECLARE @Wp1 UNIQUEIDENTIFIER = 'A1000001-0001-4000-8000-000000000001';
DECLARE @Wp2 UNIQUEIDENTIFIER = 'A1000001-0001-4000-8000-000000000002';
DECLARE @Wp3 UNIQUEIDENTIFIER = 'A1000001-0001-4000-8000-000000000003';

IF NOT EXISTS (SELECT 1 FROM ehs.WorkPermits WHERE Id = @Wp1)
INSERT INTO ehs.WorkPermits (Id, PermitNumber, WorkType, Status, Department, DepartmentName, Section, SectionName, PlantLocation, WorkInfo, DetailDescription, WorkScheduleDateFrom, WorkScheduleDateTo, SupplierCode, SupplierName, SupplierPic, SupContactNo, PoNumber, PoCost, NoPoPr, Po, Pr, IsActive, CreatedAtUtc, CreatedBy, IsDeleted)
VALUES
(@Wp1, 10001, N'HOT WORK',  N'IN PROGRESS', N'D01', N'Maintenance', N'S01', N'Facilities', N'Plant A - Confined Space Tank', N'Repair welding on tank flange', N'Hot work confined space welding. Daily atmospheric testing required.', DATEADD(day,-1,SYSUTCDATETIME()), DATEADD(day,2,SYSUTCDATETIME()), N'SUP001', N'Alpha Engineering Sdn Bhd', N'Ali Rahman', N'0123456789', N'PO-2026-001', 4500.00, 0, 1, 0, 1, SYSUTCDATETIME(), N'seed', 0),
(@Wp2, 10002, N'COLD WORK', N'APPROVED',    N'D02', N'Production',  N'S02', N'Line 1',     N'Plant B - Clean Room Corridor', N'Install cable tray', N'Cold work electrical installation.', SYSUTCDATETIME(), DATEADD(day,3,SYSUTCDATETIME()), N'SUP001', N'Alpha Engineering Sdn Bhd', N'Ali Rahman', N'0123456789', N'PR-2026-014', 1200.00, 0, 0, 1, 1, SYSUTCDATETIME(), N'seed', 0),
(@Wp3, 10003, N'HOT WORK',  N'ON HOLD',     N'D01', N'Maintenance', N'S03', N'Utilities',  N'Plant A - Boiler Room', N'Pipe cutting and welding', N'Hot work. On hold pending gas clearance.', DATEADD(day,-2,SYSUTCDATETIME()), DATEADD(day,1,SYSUTCDATETIME()), N'SUP001', N'Alpha Engineering Sdn Bhd', N'Siti Aminah', N'0198765432', N'PO-2026-009', 7800.00, 0, 1, 0, 1, SYSUTCDATETIME(), N'seed', 0);
GO

DECLARE @Wp1 UNIQUEIDENTIFIER = 'A1000001-0001-4000-8000-000000000001';
DECLARE @Wp3 UNIQUEIDENTIFIER = 'A1000001-0001-4000-8000-000000000003';

IF NOT EXISTS (SELECT 1 FROM ehs.DailyAtmosphericReadings WHERE WorkPermitId = @Wp1)
INSERT INTO ehs.DailyAtmosphericReadings (Id, WorkPermitId, ReadingDate, ReadingDateTimeUtc, OxygenContentPercent, ToxicGasH2SPpm, CarbonMonoxidePpm, CombustibleGasLelPercent, PicName, Remarks, CreatedAtUtc, CreatedBy, IsDeleted)
VALUES
(NEWID(), @Wp1, CAST(DATEADD(day,-1,SYSUTCDATETIME()) AS DATE), DATEADD(day,-1,SYSUTCDATETIME()), 20.90, 0.50, 5.00, 2.00, N'Ali Rahman', N'Day 1 pre-entry', SYSUTCDATETIME(), N'seed', 0),
(NEWID(), @Wp1, CAST(SYSUTCDATETIME() AS DATE), SYSUTCDATETIME(), 20.80, 1.20, 8.00, 3.50, N'Ali Rahman', N'Day 2 morning', SYSUTCDATETIME(), N'seed', 0),
(NEWID(), @Wp3, CAST(DATEADD(day,-2,SYSUTCDATETIME()) AS DATE), DATEADD(day,-2,SYSUTCDATETIME()), 20.50, 2.00, 12.00, 4.00, N'Siti Aminah', N'Initial reading OK', SYSUTCDATETIME(), N'seed', 0),
(NEWID(), @Wp3, CAST(DATEADD(day,-1,SYSUTCDATETIME()) AS DATE), DATEADD(day,-1,SYSUTCDATETIME()), 18.90, 12.00, 30.00, 15.00, N'Siti Aminah', N'Out of range — work on hold', SYSUTCDATETIME(), N'seed', 0);
GO
