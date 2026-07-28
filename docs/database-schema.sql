-- Change Management System - SQL Server Schema (Code First reference)
-- Generated to mirror EF Core models. Prefer migrations / EnsureCreated for runtime.

IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
CREATE TABLE dbo.AuditLogs (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    EntityName NVARCHAR(100) NOT NULL,
    EntityId NVARCHAR(50) NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    OldValues NVARCHAR(MAX) NULL,
    NewValues NVARCHAR(MAX) NULL,
    UserId NVARCHAR(100) NULL,
    UserName NVARCHAR(100) NULL,
    IpAddress NVARCHAR(64) NULL,
    CreatedDate DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);
CREATE INDEX IX_AuditLogs_EntityName ON dbo.AuditLogs(EntityName);
CREATE INDEX IX_AuditLogs_CreatedDate ON dbo.AuditLogs(CreatedDate);

IF OBJECT_ID(N'dbo.Departments', N'U') IS NULL
CREATE TABLE dbo.Departments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

IF OBJECT_ID(N'dbo.Divisions', N'U') IS NULL
CREATE TABLE dbo.Divisions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
CREATE TABLE dbo.Products (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    Name NVARCHAR(150) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
CREATE TABLE dbo.Customers (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
CREATE TABLE dbo.Roles (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL,
    CreatedDate DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
CREATE TABLE dbo.Users (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    UserName NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    EmployeeId NVARCHAR(50) NULL,
    DepartmentId UNIQUEIDENTIFIER NULL REFERENCES dbo.Departments(Id),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

IF OBJECT_ID(N'dbo.UserRoles', N'U') IS NULL
CREATE TABLE dbo.UserRoles (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    RoleId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.Roles(Id) ON DELETE CASCADE,
    CreatedDate DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_UserRoles UNIQUE (UserId, RoleId)
);

IF OBJECT_ID(N'dbo.EngineeringRequests', N'U') IS NULL
CREATE TABLE dbo.EngineeringRequests (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    ErNumber NVARCHAR(30) NOT NULL UNIQUE,
    DivisionId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.Divisions(Id),
    IsPr BIT NOT NULL,
    IsEng BIT NOT NULL,
    IsQa BIT NOT NULL,
    IsInd BIT NOT NULL,
    SubmissionCount INT NOT NULL,
    ValidationDate DATETIME2 NOT NULL,
    ExpiryDate DATETIME2 NOT NULL,
    Title NVARCHAR(250) NOT NULL,
    DepartmentId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.Departments(Id),
    ProductId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.Products(Id),
    Customer NVARCHAR(200) NOT NULL,
    Process NVARCHAR(4000) NOT NULL,
    DetailsOfEvaluation NVARCHAR(4000) NULL,
    PresentCondition NVARCHAR(4000) NULL,
    NewCondition NVARCHAR(4000) NULL,
    Merit NVARCHAR(4000) NULL,
    Demerit NVARCHAR(4000) NULL,
    MaterialDisposition NVARCHAR(4000) NULL,
    SampleQuantity INT NULL,
    TestLotIdentification INT NULL,
    TestLotDescription NVARCHAR(2000) NULL,
    ApplicableToChemicalOrMaterials BIT NOT NULL,
    SafetyDataSheet NVARCHAR(2000) NULL,
    ChemicalLabel NVARCHAR(2000) NULL,
    ChemicalClassification NVARCHAR(2000) NULL,
    ChemicalInventoryManagementSystem NVARCHAR(2000) NULL,
    Status INT NOT NULL,
    RequesterId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.Users(Id),
    LatestRejectionComments NVARCHAR(4000) NULL,
    SubmittedDate DATETIME2 NULL,
    ClosedDate DATETIME2 NULL,
    CreatedDate DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);
CREATE INDEX IX_ER_Status ON dbo.EngineeringRequests(Status);
CREATE INDEX IX_ER_Requester ON dbo.EngineeringRequests(RequesterId);
CREATE INDEX IX_ER_CreatedDate ON dbo.EngineeringRequests(CreatedDate);

IF OBJECT_ID(N'dbo.EngineeringRequestReasons', N'U') IS NULL
CREATE TABLE dbo.EngineeringRequestReasons (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    EngineeringRequestId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.EngineeringRequests(Id) ON DELETE CASCADE,
    Reason INT NOT NULL,
    OtherDescription NVARCHAR(2000) NULL,
    CreatedDate DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_ER_Reason UNIQUE (EngineeringRequestId, Reason)
);

IF OBJECT_ID(N'dbo.EngineeringRequestAttachments', N'U') IS NULL
CREATE TABLE dbo.EngineeringRequestAttachments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    EngineeringRequestId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.EngineeringRequests(Id) ON DELETE CASCADE,
    FileName NVARCHAR(260) NOT NULL,
    OriginalFileName NVARCHAR(260) NOT NULL,
    ContentType NVARCHAR(150) NULL,
    FileSizeBytes BIGINT NOT NULL,
    StoragePath NVARCHAR(500) NOT NULL,
    IsImage BIT NOT NULL,
    CreatedDate DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

IF OBJECT_ID(N'dbo.VerificationHistories', N'U') IS NULL
CREATE TABLE dbo.VerificationHistories (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    EngineeringRequestId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.EngineeringRequests(Id) ON DELETE CASCADE,
    VerifierId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.Users(Id),
    Stage INT NOT NULL,
    Action INT NOT NULL,
    Comments NVARCHAR(4000) NULL,
    ActionDate DATETIME2 NOT NULL,
    CreatedDate DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

IF OBJECT_ID(N'dbo.ApprovalHistories', N'U') IS NULL
CREATE TABLE dbo.ApprovalHistories (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    EngineeringRequestId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.EngineeringRequests(Id) ON DELETE CASCADE,
    ApproverId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.Users(Id),
    Action INT NOT NULL,
    Comments NVARCHAR(4000) NULL,
    ActionDate DATETIME2 NOT NULL,
    CreatedDate DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

IF OBJECT_ID(N'dbo.Comments', N'U') IS NULL
CREATE TABLE dbo.Comments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    EngineeringRequestId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.EngineeringRequests(Id) ON DELETE CASCADE,
    UserId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.Users(Id),
    Content NVARCHAR(4000) NOT NULL,
    Stage NVARCHAR(50) NULL,
    CreatedDate DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

IF OBJECT_ID(N'dbo.Notifications', N'U') IS NULL
CREATE TABLE dbo.Notifications (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    EngineeringRequestId UNIQUEIDENTIFIER NULL REFERENCES dbo.EngineeringRequests(Id),
    Type INT NOT NULL,
    Subject NVARCHAR(250) NOT NULL,
    Message NVARCHAR(4000) NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    EmailSent BIT NOT NULL DEFAULT 0,
    EmailSentDate DATETIME2 NULL,
    CreatedDate DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);
CREATE INDEX IX_Notifications_User_Read ON dbo.Notifications(UserId, IsRead);

IF OBJECT_ID(N'dbo.VerificationAssignments', N'U') IS NULL
CREATE TABLE dbo.VerificationAssignments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    EngineeringRequestId UNIQUEIDENTIFIER NOT NULL REFERENCES dbo.EngineeringRequests(Id) ON DELETE CASCADE,
    AssignedToUserId UNIQUEIDENTIFIER NULL REFERENCES dbo.Users(Id),
    Stage INT NOT NULL,
    IsCompleted BIT NOT NULL DEFAULT 0,
    Result INT NULL,
    CompletedDate DATETIME2 NULL,
    CreatedDate DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedDate DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);
CREATE INDEX IX_VerificationAssignments_Lookup ON dbo.VerificationAssignments(EngineeringRequestId, Stage, IsCompleted);
