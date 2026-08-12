-- MARUWA EHS System - Check Point Master + CheckSheet Items Sorting
SET QUOTED_IDENTIFIER ON;
GO

IF SCHEMA_ID('ehs') IS NULL EXEC('CREATE SCHEMA ehs');
GO

-- Check Point Master (central specs)
IF OBJECT_ID('ehs.CheckPointMasters') IS NULL
CREATE TABLE ehs.CheckPointMasters (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CheckPointMasters PRIMARY KEY,
    CheckPointName NVARCHAR(256) NOT NULL CONSTRAINT UQ_CheckPointMasters_Name UNIQUE,
    Description NVARCHAR(512) NULL,
    MinimumSpecs NVARCHAR(128) NULL,
    MaximumSpecs NVARCHAR(128) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_CheckPointMasters_IsActive DEFAULT 1,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_CheckPointMasters_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_CheckPointMasters_IsDeleted DEFAULT 0
);
GO

-- Check sheet definitions (Department / Section / Name / Frequency)
IF OBJECT_ID('ehs.CheckSheetDefinitions') IS NULL
CREATE TABLE ehs.CheckSheetDefinitions (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CheckSheetDefinitions PRIMARY KEY,
    DepartmentCode NVARCHAR(32) NOT NULL,
    DepartmentName NVARCHAR(128) NOT NULL,
    SectionCode NVARCHAR(32) NOT NULL,
    SectionName NVARCHAR(128) NOT NULL,
    MachineName NVARCHAR(256) NOT NULL,
    Frequency NVARCHAR(64) NOT NULL,
    DocumentControlNo NVARCHAR(64) NULL,
    CheckSheetDisplayName NVARCHAR(256) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_CheckSheetDefinitions_IsActive DEFAULT 1,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_CheckSheetDefinitions_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_CheckSheetDefinitions_IsDeleted DEFAULT 0
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CheckSheetDefinitions_Dept')
    CREATE INDEX IX_CheckSheetDefinitions_Dept ON ehs.CheckSheetDefinitions(DepartmentCode);
GO

IF OBJECT_ID('ehs.CheckSheetTemplateItems') IS NULL
CREATE TABLE ehs.CheckSheetTemplateItems (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CheckSheetTemplateItems PRIMARY KEY,
    CheckSheetDefinitionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_CheckSheetTemplateItems_Definition REFERENCES ehs.CheckSheetDefinitions(Id),
    Items NVARCHAR(256) NOT NULL,
    Standard NVARCHAR(256) NULL,
    CheckPoint NVARCHAR(256) NOT NULL,
    Abnormality NVARCHAR(256) NULL,
    SortOrder INT NOT NULL CONSTRAINT DF_CheckSheetTemplateItems_Sort DEFAULT 0,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_CheckSheetTemplateItems_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_CheckSheetTemplateItems_IsDeleted DEFAULT 0
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CheckSheetTemplateItems_Definition')
    CREATE INDEX IX_CheckSheetTemplateItems_Definition ON ehs.CheckSheetTemplateItems(CheckSheetDefinitionId);
GO

-- Items Sorting / CheckSheet Data (inspection results)
IF OBJECT_ID('ehs.CheckSheetData') IS NULL
CREATE TABLE ehs.CheckSheetData (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CheckSheetData PRIMARY KEY,
    DepartmentCode NVARCHAR(32) NOT NULL,
    DepartmentName NVARCHAR(128) NOT NULL,
    SectionCode NVARCHAR(32) NOT NULL,
    SectionName NVARCHAR(128) NOT NULL,
    MachineName NVARCHAR(256) NOT NULL,
    Frequency NVARCHAR(64) NOT NULL,
    CheckingDate DATE NOT NULL,
    Items NVARCHAR(256) NOT NULL,
    Standard NVARCHAR(256) NULL,
    CheckPoint NVARCHAR(256) NOT NULL,
    Abnormality NVARCHAR(256) NULL,
    BeforeRemarks NVARCHAR(MAX) NULL,
    SpecsMinimum NVARCHAR(128) NULL,
    SpecsMaximum NVARCHAR(128) NULL,
    ActualSpecs NVARCHAR(128) NULL,
    Remarks NVARCHAR(MAX) NULL,
    -- Legacy single image (existing compatibility) + up to 4 images
    Image NVARCHAR(512) NULL,
    Image1 NVARCHAR(512) NULL,
    Image2 NVARCHAR(512) NULL,
    Image3 NVARCHAR(512) NULL,
    Image4 NVARCHAR(512) NULL,
    Priority NVARCHAR(32) NULL,
    Status NVARCHAR(16) NULL,
    Score INT NULL,
    AuditCategory NVARCHAR(128) NULL,
    Ok BIT NOT NULL CONSTRAINT DF_CheckSheetData_Ok DEFAULT 0,
    Ng BIT NOT NULL CONSTRAINT DF_CheckSheetData_Ng DEFAULT 0,
    Change BIT NOT NULL CONSTRAINT DF_CheckSheetData_Change DEFAULT 0,
    CreatedByName NVARCHAR(128) NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_CheckSheetData_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_CheckSheetData_IsDeleted DEFAULT 0
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CheckSheetData_Lookup')
    CREATE INDEX IX_CheckSheetData_Lookup ON ehs.CheckSheetData(DepartmentCode, SectionCode, MachineName, Frequency, CheckingDate);
GO

-- Migration helper: if an older CheckSheetData table exists without new columns, add them.
IF OBJECT_ID('ehs.CheckSheetData') IS NOT NULL
BEGIN
    IF COL_LENGTH('ehs.CheckSheetData', 'BeforeRemarks') IS NULL
        ALTER TABLE ehs.CheckSheetData ADD BeforeRemarks NVARCHAR(MAX) NULL;
    IF COL_LENGTH('ehs.CheckSheetData', 'SpecsMinimum') IS NULL
        ALTER TABLE ehs.CheckSheetData ADD SpecsMinimum NVARCHAR(128) NULL;
    IF COL_LENGTH('ehs.CheckSheetData', 'SpecsMaximum') IS NULL
        ALTER TABLE ehs.CheckSheetData ADD SpecsMaximum NVARCHAR(128) NULL;
    IF COL_LENGTH('ehs.CheckSheetData', 'ActualSpecs') IS NULL
        ALTER TABLE ehs.CheckSheetData ADD ActualSpecs NVARCHAR(128) NULL;
    IF COL_LENGTH('ehs.CheckSheetData', 'Image1') IS NULL
        ALTER TABLE ehs.CheckSheetData ADD Image1 NVARCHAR(512) NULL;
    IF COL_LENGTH('ehs.CheckSheetData', 'Image2') IS NULL
        ALTER TABLE ehs.CheckSheetData ADD Image2 NVARCHAR(512) NULL;
    IF COL_LENGTH('ehs.CheckSheetData', 'Image3') IS NULL
        ALTER TABLE ehs.CheckSheetData ADD Image3 NVARCHAR(512) NULL;
    IF COL_LENGTH('ehs.CheckSheetData', 'Image4') IS NULL
        ALTER TABLE ehs.CheckSheetData ADD Image4 NVARCHAR(512) NULL;
    IF COL_LENGTH('ehs.CheckSheetData', 'Score') IS NULL
        ALTER TABLE ehs.CheckSheetData ADD Score INT NULL;
END
GO

-- Backfill Image1 from legacy Image where Image1 is empty
UPDATE ehs.CheckSheetData
SET Image1 = Image
WHERE Image IS NOT NULL AND Image <> '' AND (Image1 IS NULL OR Image1 = '');
GO

-- Seed Check Point Master
DECLARE @cp1 UNIQUEIDENTIFIER = 'A1000001-0000-4000-8000-000000000001';
DECLARE @cp2 UNIQUEIDENTIFIER = 'A1000001-0000-4000-8000-000000000002';
DECLARE @cp3 UNIQUEIDENTIFIER = 'A1000001-0000-4000-8000-000000000003';
DECLARE @cp4 UNIQUEIDENTIFIER = 'A1000001-0000-4000-8000-000000000004';

IF NOT EXISTS (SELECT 1 FROM ehs.CheckPointMasters WHERE CheckPointName = N'TEMPERATURE CHECK')
INSERT INTO ehs.CheckPointMasters (Id, CheckPointName, Description, MinimumSpecs, MaximumSpecs, IsActive, CreatedAtUtc, CreatedBy, IsDeleted)
VALUES (@cp1, N'TEMPERATURE CHECK', N'Ambient / process temperature', N'20°C', N'30°C', 1, SYSUTCDATETIME(), N'seed', 0);

IF NOT EXISTS (SELECT 1 FROM ehs.CheckPointMasters WHERE CheckPointName = N'PRESSURE CHECK')
INSERT INTO ehs.CheckPointMasters (Id, CheckPointName, Description, MinimumSpecs, MaximumSpecs, IsActive, CreatedAtUtc, CreatedBy, IsDeleted)
VALUES (@cp2, N'PRESSURE CHECK', N'System pressure', N'5 bar', N'10 bar', 1, SYSUTCDATETIME(), N'seed', 0);

IF NOT EXISTS (SELECT 1 FROM ehs.CheckPointMasters WHERE CheckPointName = N'WATER LEVEL')
INSERT INTO ehs.CheckPointMasters (Id, CheckPointName, Description, MinimumSpecs, MaximumSpecs, IsActive, CreatedAtUtc, CreatedBy, IsDeleted)
VALUES (@cp3, N'WATER LEVEL', N'Tank water level', N'50%', N'80%', 1, SYSUTCDATETIME(), N'seed', 0);

IF NOT EXISTS (SELECT 1 FROM ehs.CheckPointMasters WHERE CheckPointName = N'EMERGENCY LAYOUT IS DISPLAY?')
INSERT INTO ehs.CheckPointMasters (Id, CheckPointName, Description, MinimumSpecs, MaximumSpecs, IsActive, CreatedAtUtc, CreatedBy, IsDeleted)
VALUES (@cp4, N'EMERGENCY LAYOUT IS DISPLAY?', N'Visual check — no numeric specs', NULL, NULL, 1, SYSUTCDATETIME(), N'seed', 0);

-- Seed Check Sheet Definition + template items
DECLARE @def1 UNIQUEIDENTIFIER = 'B1000001-0000-4000-8000-000000000001';

IF NOT EXISTS (SELECT 1 FROM ehs.CheckSheetDefinitions WHERE Id = @def1)
INSERT INTO ehs.CheckSheetDefinitions (Id, DepartmentCode, DepartmentName, SectionCode, SectionName, MachineName, Frequency, DocumentControlNo, CheckSheetDisplayName, IsActive, CreatedAtUtc, CreatedBy, IsDeleted)
VALUES (@def1, N'EHS', N'EHS Department', N'FF', N'Fire Fighting', N'FIRE FIGHTING SYSTEM/EQUIPMENT', N'Daily', N'DCN-EHS-001', N'Fire Fighting Daily Check', 1, SYSUTCDATETIME(), N'seed', 0);

IF NOT EXISTS (SELECT 1 FROM ehs.CheckSheetTemplateItems WHERE CheckSheetDefinitionId = @def1 AND CheckPoint = N'EMERGENCY LAYOUT IS DISPLAY?')
INSERT INTO ehs.CheckSheetTemplateItems (Id, CheckSheetDefinitionId, Items, Standard, CheckPoint, Abnormality, SortOrder, CreatedAtUtc, CreatedBy, IsDeleted)
VALUES
 (NEWID(), @def1, N'FIRE FIGHTING SYSTEM/EQUIPMENT', N'Visible', N'EMERGENCY LAYOUT IS DISPLAY?', N'Missing/blocked', 1, SYSUTCDATETIME(), N'seed', 0),
 (NEWID(), @def1, N'FIRE FIGHTING SYSTEM/EQUIPMENT', N'20-30C', N'TEMPERATURE CHECK', N'Out of range', 2, SYSUTCDATETIME(), N'seed', 0),
 (NEWID(), @def1, N'MACHINE MANAGEMENT', N'5-10 bar', N'PRESSURE CHECK', N'Out of range', 3, SYSUTCDATETIME(), N'seed', 0);

GO
