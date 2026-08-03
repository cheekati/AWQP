-- MARUWA EHS System - PPE module (Employees, PPE Master, Request, Issue, Inventory)
SET QUOTED_IDENTIFIER ON;
GO

IF SCHEMA_ID('ehs') IS NULL EXEC('CREATE SCHEMA ehs');
GO

IF OBJECT_ID('ehs.Employees') IS NULL
CREATE TABLE ehs.Employees (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Employees PRIMARY KEY,
    EmployeeNumber NVARCHAR(32) NOT NULL CONSTRAINT UQ_Employees_Number UNIQUE,
    FullName NVARCHAR(128) NOT NULL,
    Department NVARCHAR(64) NOT NULL,
    Designation NVARCHAR(64) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Employees_IsActive DEFAULT 1,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Employees_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Employees_IsDeleted DEFAULT 0
);
GO

IF OBJECT_ID('ehs.PpeItems') IS NULL
CREATE TABLE ehs.PpeItems (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PpeItems PRIMARY KEY,
    ItemCode NVARCHAR(32) NOT NULL CONSTRAINT UQ_PpeItems_Code UNIQUE,
    Name NVARCHAR(128) NOT NULL,
    Category NVARCHAR(64) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    LifetimeMonths INT NOT NULL,
    UnitOfMeasure NVARCHAR(16) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_PpeItems_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_PpeItems_IsDeleted DEFAULT 0
);
GO

IF OBJECT_ID('ehs.PpeRequests') IS NULL
CREATE TABLE ehs.PpeRequests (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PpeRequests PRIMARY KEY,
    RequestNumber NVARCHAR(32) NOT NULL CONSTRAINT UQ_PpeRequests_Number UNIQUE,
    EmployeeId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_PpeRequests_Employee REFERENCES ehs.Employees(Id),
    PpeItemId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_PpeRequests_Item REFERENCES ehs.PpeItems(Id),
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    RequesterSignature NVARCHAR(MAX) NULL,
    Status INT NOT NULL,
    RequestedAtUtc DATETIME2 NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_PpeRequests_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_PpeRequests_IsDeleted DEFAULT 0
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PpeRequests_EmployeeId')
    CREATE INDEX IX_PpeRequests_EmployeeId ON ehs.PpeRequests(EmployeeId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PpeRequests_PpeItemId')
    CREATE INDEX IX_PpeRequests_PpeItemId ON ehs.PpeRequests(PpeItemId);
GO

IF OBJECT_ID('ehs.PpeIssues') IS NULL
CREATE TABLE ehs.PpeIssues (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PpeIssues PRIMARY KEY,
    PpeRequestId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_PpeIssues_Request REFERENCES ehs.PpeRequests(Id),
    IssuedQuantity INT NOT NULL,
    ReceiverSignature NVARCHAR(MAX) NOT NULL,
    IssuedBy NVARCHAR(128) NOT NULL,
    IssuedAtUtc DATETIME2 NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_PpeIssues_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_PpeIssues_IsDeleted DEFAULT 0,
    CONSTRAINT UQ_PpeIssues_Request UNIQUE (PpeRequestId)
);
GO

IF OBJECT_ID('ehs.PpeStockEntries') IS NULL
CREATE TABLE ehs.PpeStockEntries (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PpeStockEntries PRIMARY KEY,
    PpeItemId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_PpeStockEntries_Item REFERENCES ehs.PpeItems(Id),
    AddedQuantity INT NOT NULL,
    Remarks NVARCHAR(256) NULL,
    EntryDateUtc DATETIME2 NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_PpeStockEntries_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_PpeStockEntries_IsDeleted DEFAULT 0
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PpeStockEntries_PpeItemId')
    CREATE INDEX IX_PpeStockEntries_PpeItemId ON ehs.PpeStockEntries(PpeItemId);
GO

-- Seed employees
IF NOT EXISTS (SELECT 1 FROM ehs.Employees)
INSERT INTO ehs.Employees (Id, EmployeeNumber, FullName, Department, Designation, IsActive, CreatedBy) VALUES
 ('22222222-2222-2222-2222-222222220001', 'EMP-1001', 'Loga Nathan',     'Management', 'Manager',        1, 'seed'),
 ('22222222-2222-2222-2222-222222220002', 'EMP-1002', 'Koteswar Rao',    'EHS',        'Team Lead',      1, 'seed'),
 ('22222222-2222-2222-2222-222222220003', 'EMP-1003', 'Jaya Lakshmi',    'EHS',        'Safety Officer', 1, 'seed'),
 ('22222222-2222-2222-2222-222222220004', 'EMP-1004', 'Fathima Begum',   'EHS',        'Safety Officer', 1, 'seed'),
 ('22222222-2222-2222-2222-222222220005', 'EMP-1005', 'Ravi Kumar',      'Production', 'Operator',       1, 'seed');
GO

-- Seed PPE master items (with price)
IF NOT EXISTS (SELECT 1 FROM ehs.PpeItems)
INSERT INTO ehs.PpeItems (Id, ItemCode, Name, Category, Price, LifetimeMonths, UnitOfMeasure, CreatedBy) VALUES
 ('11111111-1111-1111-1111-111111110001', 'PPE-HELMET',  'Safety Helmet',        'Head',        250.00, 24, 'EA', 'seed'),
 ('11111111-1111-1111-1111-111111110002', 'PPE-GLOVES',  'Nitrile Gloves',       'Hand',         45.00,  6, 'PR', 'seed'),
 ('11111111-1111-1111-1111-111111110003', 'PPE-GOGGLES', 'Safety Goggles',       'Eye',         120.00, 12, 'EA', 'seed'),
 ('11111111-1111-1111-1111-111111110004', 'PPE-MASK',    'N95 Respirator Mask',  'Respiratory',  35.00,  1, 'EA', 'seed'),
 ('11111111-1111-1111-1111-111111110005', 'PPE-SUIT',    'Clean Room Suit',      'Body',        600.00, 12, 'EA', 'seed');
GO

-- Seed opening stock so inventory shows an existing quantity
IF NOT EXISTS (SELECT 1 FROM ehs.PpeStockEntries)
INSERT INTO ehs.PpeStockEntries (Id, PpeItemId, AddedQuantity, Remarks, EntryDateUtc, CreatedBy) VALUES
 (NEWID(), '11111111-1111-1111-1111-111111110001', 50,  'Opening stock', SYSUTCDATETIME(), 'seed'),
 (NEWID(), '11111111-1111-1111-1111-111111110002', 500, 'Opening stock', SYSUTCDATETIME(), 'seed'),
 (NEWID(), '11111111-1111-1111-1111-111111110003', 80,  'Opening stock', SYSUTCDATETIME(), 'seed'),
 (NEWID(), '11111111-1111-1111-1111-111111110004', 300, 'Opening stock', SYSUTCDATETIME(), 'seed'),
 (NEWID(), '11111111-1111-1111-1111-111111110005', 40,  'Opening stock', SYSUTCDATETIME(), 'seed');
GO
