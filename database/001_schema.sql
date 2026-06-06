CREATE SCHEMA auth;
GO
CREATE SCHEMA erp;
GO

CREATE TABLE auth.Users (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Users PRIMARY KEY DEFAULT NEWID(),
    UserName NVARCHAR(256) NOT NULL,
    NormalizedUserName NVARCHAR(256) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    NormalizedEmail NVARCHAR(256) NOT NULL,
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    PasswordHash NVARCHAR(MAX) NULL,
    SecurityStamp NVARCHAR(MAX) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    PhoneNumber NVARCHAR(64) NULL,
    PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    LockoutEnd DATETIMEOFFSET NULL,
    LockoutEnabled BIT NOT NULL DEFAULT 1,
    AccessFailedCount INT NOT NULL DEFAULT 0,
    DisplayName NVARCHAR(160) NOT NULL,
    Department NVARCHAR(80) NOT NULL,
    MustChangePassword BIT NOT NULL DEFAULT 0,
    LastLoginAt DATETIMEOFFSET NULL
);
CREATE UNIQUE INDEX UX_Users_NormalizedUserName ON auth.Users(NormalizedUserName);
CREATE INDEX IX_Users_NormalizedEmail ON auth.Users(NormalizedEmail);
GO

CREATE TABLE auth.Roles (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Roles PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(256) NOT NULL,
    NormalizedName NVARCHAR(256) NOT NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL
);
CREATE UNIQUE INDEX UX_Roles_NormalizedName ON auth.Roles(NormalizedName);
GO

CREATE TABLE auth.UserRoles (
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES auth.Users(Id),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES auth.Roles(Id)
);
GO

CREATE TABLE auth.RefreshTokens (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RefreshTokens PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    TokenHash NVARCHAR(256) NOT NULL,
    ExpiresAt DATETIMEOFFSET NOT NULL,
    RevokedAt DATETIMEOFFSET NULL,
    ReplacedByTokenHash NVARCHAR(256) NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES auth.Users(Id)
);
CREATE INDEX IX_RefreshTokens_UserId_TokenHash ON auth.RefreshTokens(UserId, TokenHash);
GO

CREATE TABLE erp.Customers (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Customers PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(32) NOT NULL,
    Name NVARCHAR(160) NOT NULL,
    Description NVARCHAR(512) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    LegalName NVARCHAR(240) NOT NULL,
    TaxNumber NVARCHAR(80) NULL,
    Website NVARCHAR(240) NULL,
    PaymentTerms NVARCHAR(80) NOT NULL,
    CreditStatus NVARCHAR(40) NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL
);
CREATE UNIQUE INDEX UX_Customers_Code ON erp.Customers(Code) WHERE IsDeleted = 0;
GO

CREATE TABLE erp.CustomerContacts (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerContacts PRIMARY KEY DEFAULT NEWID(),
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(160) NOT NULL,
    JobTitle NVARCHAR(120) NULL,
    Email NVARCHAR(256) NOT NULL,
    Phone NVARCHAR(80) NULL,
    IsPrimary BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_CustomerContacts_Customers FOREIGN KEY(CustomerId) REFERENCES erp.Customers(Id)
);
CREATE INDEX IX_CustomerContacts_CustomerId ON erp.CustomerContacts(CustomerId);
GO

CREATE TABLE erp.CustomerAddresses (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerAddresses PRIMARY KEY DEFAULT NEWID(),
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    AddressType NVARCHAR(32) NOT NULL,
    Line1 NVARCHAR(240) NOT NULL,
    Line2 NVARCHAR(240) NULL,
    City NVARCHAR(120) NOT NULL,
    StateProvince NVARCHAR(120) NULL,
    PostalCode NVARCHAR(40) NULL,
    Country NVARCHAR(120) NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_CustomerAddresses_Customers FOREIGN KEY(CustomerId) REFERENCES erp.Customers(Id)
);
GO

CREATE TABLE erp.CustomerIndustries (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerIndustries PRIMARY KEY DEFAULT NEWID(),
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    Segment INT NOT NULL,
    Application NVARCHAR(160) NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT CK_CustomerIndustries_Segment CHECK (Segment BETWEEN 1 AND 4),
    CONSTRAINT FK_CustomerIndustries_Customers FOREIGN KEY(CustomerId) REFERENCES erp.Customers(Id)
);
GO

CREATE TABLE erp.CustomerDocuments (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerDocuments PRIMARY KEY DEFAULT NEWID(),
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    FileName NVARCHAR(260) NOT NULL,
    StorageUri NVARCHAR(1024) NOT NULL,
    DocumentType INT NOT NULL,
    UploadedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_CustomerDocuments_Customers FOREIGN KEY(CustomerId) REFERENCES erp.Customers(Id)
);
GO

CREATE TABLE erp.ProductCategories (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ProductCategories PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(32) NOT NULL,
    Name NVARCHAR(160) NOT NULL,
    Description NVARCHAR(512) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    Segment INT NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT CK_ProductCategories_Segment CHECK (Segment BETWEEN 1 AND 4)
);
CREATE UNIQUE INDEX UX_ProductCategories_Code ON erp.ProductCategories(Code);
GO

CREATE TABLE erp.MaterialGrades (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MaterialGrades PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(32) NOT NULL,
    Name NVARCHAR(160) NOT NULL,
    Description NVARCHAR(512) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    MinimumPurityPercent DECIMAL(18,4) NOT NULL,
    SupplierSpecification NVARCHAR(512) NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL
);
CREATE UNIQUE INDEX UX_MaterialGrades_Code ON erp.MaterialGrades(Code);
GO

CREATE TABLE erp.Products (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Products PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(40) NOT NULL,
    Name NVARCHAR(180) NOT NULL,
    Description NVARCHAR(512) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    ProductCategoryId UNIQUEIDENTIFIER NOT NULL,
    MaterialGradeId UNIQUEIDENTIFIER NOT NULL,
    ProductFamily NVARCHAR(120) NOT NULL,
    DefaultUom NVARCHAR(16) NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_Products_ProductCategories FOREIGN KEY(ProductCategoryId) REFERENCES erp.ProductCategories(Id),
    CONSTRAINT FK_Products_MaterialGrades FOREIGN KEY(MaterialGradeId) REFERENCES erp.MaterialGrades(Id)
);
CREATE UNIQUE INDEX UX_Products_Code ON erp.Products(Code) WHERE IsDeleted = 0;
GO

CREATE TABLE erp.ProductSpecifications (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ProductSpecifications PRIMARY KEY DEFAULT NEWID(),
    ProductId UNIQUEIDENTIFIER NOT NULL,
    ParameterName NVARCHAR(120) NOT NULL,
    NominalValue NVARCHAR(80) NOT NULL,
    LowerTolerance NVARCHAR(80) NULL,
    UpperTolerance NVARCHAR(80) NULL,
    UnitOfMeasure NVARCHAR(32) NULL,
    IsCriticalToQuality BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_ProductSpecifications_Products FOREIGN KEY(ProductId) REFERENCES erp.Products(Id)
);
GO

CREATE TABLE erp.ProductDimensions (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ProductDimensions PRIMARY KEY DEFAULT NEWID(),
    ProductId UNIQUEIDENTIFIER NOT NULL,
    LengthMm DECIMAL(18,4) NOT NULL,
    WidthMm DECIMAL(18,4) NOT NULL,
    HeightMm DECIMAL(18,4) NOT NULL,
    InnerDiameterMm DECIMAL(18,4) NOT NULL,
    OuterDiameterMm DECIMAL(18,4) NOT NULL,
    WallThicknessMm DECIMAL(18,4) NOT NULL,
    ToleranceClass NVARCHAR(40) NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_ProductDimensions_Products FOREIGN KEY(ProductId) REFERENCES erp.Products(Id)
);
GO

CREATE TABLE erp.ProductRevisions (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ProductRevisions PRIMARY KEY DEFAULT NEWID(),
    ProductId UNIQUEIDENTIFIER NOT NULL,
    RevisionNumber NVARCHAR(20) NOT NULL,
    ChangeSummary NVARCHAR(1000) NOT NULL,
    EffectiveDate DATETIMEOFFSET NOT NULL,
    IsCurrent BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_ProductRevisions_Products FOREIGN KEY(ProductId) REFERENCES erp.Products(Id)
);
CREATE UNIQUE INDEX UX_ProductRevisions_Current ON erp.ProductRevisions(ProductId) WHERE IsCurrent = 1 AND IsDeleted = 0;
GO

CREATE TABLE erp.EngineeringDrawings (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EngineeringDrawings PRIMARY KEY DEFAULT NEWID(),
    ProductId UNIQUEIDENTIFIER NOT NULL,
    DrawingNumber NVARCHAR(80) NOT NULL,
    RevisionNumber NVARCHAR(20) NOT NULL,
    FileName NVARCHAR(260) NOT NULL,
    StorageUri NVARCHAR(1024) NOT NULL,
    IsReleased BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_EngineeringDrawings_Products FOREIGN KEY(ProductId) REFERENCES erp.Products(Id)
);
CREATE INDEX IX_EngineeringDrawings_DrawingRevision ON erp.EngineeringDrawings(DrawingNumber, RevisionNumber);
GO

CREATE TABLE erp.EngineeringChangeRequests (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EngineeringChangeRequests PRIMARY KEY DEFAULT NEWID(),
    ProductId UNIQUEIDENTIFIER NOT NULL,
    RequestNumber NVARCHAR(80) NOT NULL,
    Reason NVARCHAR(1000) NOT NULL,
    ImpactAssessment NVARCHAR(2000) NOT NULL,
    Status INT NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_EngineeringChangeRequests_Products FOREIGN KEY(ProductId) REFERENCES erp.Products(Id)
);
CREATE UNIQUE INDEX UX_ECR_RequestNumber ON erp.EngineeringChangeRequests(RequestNumber);
GO

CREATE TABLE erp.RequestForQuotes (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RequestForQuotes PRIMARY KEY DEFAULT NEWID(),
    RfqNumber NVARCHAR(80) NOT NULL,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL CONSTRAINT CK_RFQ_Quantity CHECK (Quantity > 0),
    RequiredDate DATE NOT NULL,
    Status INT NOT NULL,
    TechnicalRequirements NVARCHAR(2000) NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_RFQ_Customers FOREIGN KEY(CustomerId) REFERENCES erp.Customers(Id),
    CONSTRAINT FK_RFQ_Products FOREIGN KEY(ProductId) REFERENCES erp.Products(Id)
);
CREATE UNIQUE INDEX UX_RFQ_RfqNumber ON erp.RequestForQuotes(RfqNumber);
GO

CREATE TABLE erp.Quotations (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Quotations PRIMARY KEY DEFAULT NEWID(),
    QuotationNumber NVARCHAR(80) NOT NULL,
    RequestForQuoteId UNIQUEIDENTIFIER NOT NULL,
    UnitPrice DECIMAL(18,4) NOT NULL,
    ToolingCharge DECIMAL(18,4) NOT NULL DEFAULT 0,
    LeadTimeDays DECIMAL(18,2) NOT NULL,
    ValidUntil DATE NOT NULL,
    Status INT NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_Quotations_RFQ FOREIGN KEY(RequestForQuoteId) REFERENCES erp.RequestForQuotes(Id)
);
CREATE UNIQUE INDEX UX_Quotations_Number ON erp.Quotations(QuotationNumber);
GO

CREATE TABLE erp.SalesOrders (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SalesOrders PRIMARY KEY DEFAULT NEWID(),
    SalesOrderNumber NVARCHAR(80) NOT NULL,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    QuotationId UNIQUEIDENTIFIER NULL,
    OrderDate DATE NOT NULL,
    RequestedShipDate DATE NOT NULL,
    Status INT NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_SalesOrders_Customers FOREIGN KEY(CustomerId) REFERENCES erp.Customers(Id),
    CONSTRAINT FK_SalesOrders_Quotations FOREIGN KEY(QuotationId) REFERENCES erp.Quotations(Id)
);
CREATE UNIQUE INDEX UX_SalesOrders_Number ON erp.SalesOrders(SalesOrderNumber);
GO

CREATE TABLE erp.SalesOrderLines (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SalesOrderLines PRIMARY KEY DEFAULT NEWID(),
    SalesOrderId UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(18,4) NOT NULL,
    ProductRevision NVARCHAR(20) NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_SalesOrderLines_SalesOrders FOREIGN KEY(SalesOrderId) REFERENCES erp.SalesOrders(Id),
    CONSTRAINT FK_SalesOrderLines_Products FOREIGN KEY(ProductId) REFERENCES erp.Products(Id)
);
GO

CREATE TABLE erp.SalesApprovals (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SalesApprovals PRIMARY KEY DEFAULT NEWID(),
    SalesOrderId UNIQUEIDENTIFIER NOT NULL,
    ApprovalLevel NVARCHAR(80) NOT NULL,
    ApproverUserId NVARCHAR(128) NOT NULL,
    Status INT NOT NULL,
    ApprovedAt DATETIMEOFFSET NULL,
    Remarks NVARCHAR(1000) NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_SalesApprovals_SalesOrders FOREIGN KEY(SalesOrderId) REFERENCES erp.SalesOrders(Id)
);
GO

CREATE TABLE erp.Machines (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Machines PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(32) NOT NULL,
    Name NVARCHAR(160) NOT NULL,
    Description NVARCHAR(512) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    WorkCenter NVARCHAR(80) NOT NULL,
    RatedCapacityPerHour DECIMAL(18,4) NOT NULL,
    IsCleanRoomQualified BIT NOT NULL DEFAULT 0,
    LastCalibrationAt DATETIMEOFFSET NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL
);
CREATE UNIQUE INDEX UX_Machines_Code ON erp.Machines(Code);
GO

CREATE TABLE erp.Shifts (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Shifts PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(32) NOT NULL,
    Name NVARCHAR(160) NOT NULL,
    Description NVARCHAR(512) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    StartsAt TIME NOT NULL,
    EndsAt TIME NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL
);
GO

CREATE TABLE erp.WorkOrders (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkOrders PRIMARY KEY DEFAULT NEWID(),
    WorkOrderNumber NVARCHAR(80) NOT NULL,
    SalesOrderLineId UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    BatchNumber NVARCHAR(64) NOT NULL,
    LotNumber NVARCHAR(64) NOT NULL,
    QuantityPlanned DECIMAL(18,4) NOT NULL CHECK (QuantityPlanned > 0),
    QuantityProduced DECIMAL(18,4) NOT NULL DEFAULT 0,
    QuantityRejected DECIMAL(18,4) NOT NULL DEFAULT 0,
    YieldPercentage DECIMAL(18,4) NOT NULL DEFAULT 0,
    ManufacturingDate DATE NOT NULL,
    Status INT NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_WorkOrders_SalesOrderLines FOREIGN KEY(SalesOrderLineId) REFERENCES erp.SalesOrderLines(Id),
    CONSTRAINT FK_WorkOrders_Products FOREIGN KEY(ProductId) REFERENCES erp.Products(Id)
);
CREATE UNIQUE INDEX UX_WorkOrders_Number ON erp.WorkOrders(WorkOrderNumber);
CREATE INDEX IX_WorkOrders_BatchLot ON erp.WorkOrders(BatchNumber, LotNumber);
GO

CREATE TABLE erp.ProductionSchedules (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ProductionSchedules PRIMARY KEY DEFAULT NEWID(),
    WorkOrderId UNIQUEIDENTIFIER NOT NULL,
    PlannedStart DATETIMEOFFSET NOT NULL,
    PlannedEnd DATETIMEOFFSET NOT NULL,
    ShiftId UNIQUEIDENTIFIER NULL,
    Priority NVARCHAR(32) NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_ProductionSchedules_WorkOrders FOREIGN KEY(WorkOrderId) REFERENCES erp.WorkOrders(Id),
    CONSTRAINT FK_ProductionSchedules_Shifts FOREIGN KEY(ShiftId) REFERENCES erp.Shifts(Id)
);
GO

CREATE TABLE erp.ResourceAllocations (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ResourceAllocations PRIMARY KEY DEFAULT NEWID(),
    WorkOrderId UNIQUEIDENTIFIER NOT NULL,
    MachineId UNIQUEIDENTIFIER NOT NULL,
    OperatorUserId NVARCHAR(128) NOT NULL,
    AllocatedFrom DATETIMEOFFSET NOT NULL,
    AllocatedTo DATETIMEOFFSET NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_ResourceAllocations_WorkOrders FOREIGN KEY(WorkOrderId) REFERENCES erp.WorkOrders(Id),
    CONSTRAINT FK_ResourceAllocations_Machines FOREIGN KEY(MachineId) REFERENCES erp.Machines(Id)
);
CREATE INDEX IX_ResourceAllocations_MachineTime ON erp.ResourceAllocations(MachineId, AllocatedFrom, AllocatedTo);
GO

CREATE TABLE erp.ManufacturingOperations (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ManufacturingOperations PRIMARY KEY DEFAULT NEWID(),
    WorkOrderId UNIQUEIDENTIFIER NOT NULL,
    Stage INT NOT NULL,
    Sequence INT NOT NULL,
    OperatorUserId NVARCHAR(128) NOT NULL,
    MachineId UNIQUEIDENTIFIER NULL,
    StartTime DATETIMEOFFSET NULL,
    EndTime DATETIMEOFFSET NULL,
    QuantityPlanned DECIMAL(18,4) NOT NULL,
    QuantityProduced DECIMAL(18,4) NOT NULL DEFAULT 0,
    QuantityRejected DECIMAL(18,4) NOT NULL DEFAULT 0,
    YieldPercentage DECIMAL(18,4) NOT NULL DEFAULT 0,
    Status INT NOT NULL,
    Remarks NVARCHAR(1000) NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT CK_ManufacturingOperations_Stage CHECK (Stage BETWEEN 1 AND 10),
    CONSTRAINT FK_ManufacturingOperations_WorkOrders FOREIGN KEY(WorkOrderId) REFERENCES erp.WorkOrders(Id),
    CONSTRAINT FK_ManufacturingOperations_Machines FOREIGN KEY(MachineId) REFERENCES erp.Machines(Id)
);
CREATE INDEX IX_ManufacturingOperations_WorkOrderSequence ON erp.ManufacturingOperations(WorkOrderId, Sequence);
GO

CREATE TABLE erp.InspectionRecords (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_InspectionRecords PRIMARY KEY DEFAULT NEWID(),
    InspectionNumber NVARCHAR(80) NOT NULL,
    InspectionType INT NOT NULL,
    WorkOrderId UNIQUEIDENTIFIER NULL,
    ManufacturingOperationId UNIQUEIDENTIFIER NULL,
    InspectorUserId NVARCHAR(128) NOT NULL,
    InspectedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    Disposition INT NOT NULL,
    Remarks NVARCHAR(1000) NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_InspectionRecords_WorkOrders FOREIGN KEY(WorkOrderId) REFERENCES erp.WorkOrders(Id),
    CONSTRAINT FK_InspectionRecords_Operations FOREIGN KEY(ManufacturingOperationId) REFERENCES erp.ManufacturingOperations(Id)
);
CREATE UNIQUE INDEX UX_InspectionRecords_Number ON erp.InspectionRecords(InspectionNumber);
GO

CREATE TABLE erp.InspectionParameterResults (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_InspectionParameterResults PRIMARY KEY DEFAULT NEWID(),
    InspectionRecordId UNIQUEIDENTIFIER NOT NULL,
    ParameterName NVARCHAR(120) NOT NULL,
    Specification NVARCHAR(160) NOT NULL,
    ActualValue NVARCHAR(160) NOT NULL,
    UnitOfMeasure NVARCHAR(32) NULL,
    IsPass BIT NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_InspectionParameterResults_InspectionRecords FOREIGN KEY(InspectionRecordId) REFERENCES erp.InspectionRecords(Id)
);
GO

CREATE TABLE erp.Defects (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Defects PRIMARY KEY DEFAULT NEWID(),
    InspectionRecordId UNIQUEIDENTIFIER NOT NULL,
    DefectCode NVARCHAR(40) NOT NULL,
    Description NVARCHAR(1000) NOT NULL,
    Severity INT NOT NULL,
    QuantityAffected DECIMAL(18,4) NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_Defects_InspectionRecords FOREIGN KEY(InspectionRecordId) REFERENCES erp.InspectionRecords(Id)
);
CREATE INDEX IX_Defects_CodeSeverity ON erp.Defects(DefectCode, Severity);
GO

CREATE TABLE erp.NonConformanceReports (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_NonConformanceReports PRIMARY KEY DEFAULT NEWID(),
    NcrNumber NVARCHAR(80) NOT NULL,
    InspectionRecordId UNIQUEIDENTIFIER NOT NULL,
    ProblemStatement NVARCHAR(2000) NOT NULL,
    ContainmentAction NVARCHAR(2000) NOT NULL,
    Status INT NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_NCR_InspectionRecords FOREIGN KEY(InspectionRecordId) REFERENCES erp.InspectionRecords(Id)
);
CREATE UNIQUE INDEX UX_NCR_Number ON erp.NonConformanceReports(NcrNumber);
GO

CREATE TABLE erp.CorrectivePreventiveActions (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CorrectivePreventiveActions PRIMARY KEY DEFAULT NEWID(),
    CapaNumber NVARCHAR(80) NOT NULL,
    NonConformanceReportId UNIQUEIDENTIFIER NOT NULL,
    ActionPlan NVARCHAR(2000) NOT NULL,
    OwnerUserId NVARCHAR(128) NOT NULL,
    DueDate DATE NOT NULL,
    Status INT NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_CAPA_NCR FOREIGN KEY(NonConformanceReportId) REFERENCES erp.NonConformanceReports(Id)
);
CREATE UNIQUE INDEX UX_CAPA_Number ON erp.CorrectivePreventiveActions(CapaNumber);
GO

CREATE TABLE erp.RootCauseAnalyses (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RootCauseAnalyses PRIMARY KEY DEFAULT NEWID(),
    CorrectivePreventiveActionId UNIQUEIDENTIFIER NOT NULL,
    Method NVARCHAR(80) NOT NULL,
    RootCause NVARCHAR(2000) NOT NULL,
    VerificationEvidence NVARCHAR(2000) NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_RootCauseAnalyses_CAPA FOREIGN KEY(CorrectivePreventiveActionId) REFERENCES erp.CorrectivePreventiveActions(Id)
);
GO

CREATE TABLE erp.CleanRoomAreas (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CleanRoomAreas PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(32) NOT NULL,
    Name NVARCHAR(160) NOT NULL,
    Description NVARCHAR(512) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsoClass NVARCHAR(40) NOT NULL,
    TemperatureLowerC DECIMAL(18,4) NOT NULL,
    TemperatureUpperC DECIMAL(18,4) NOT NULL,
    HumidityLowerPercent DECIMAL(18,4) NOT NULL,
    HumidityUpperPercent DECIMAL(18,4) NOT NULL,
    ParticleLimitPerCubicFoot INT NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL
);
CREATE UNIQUE INDEX UX_CleanRoomAreas_Code ON erp.CleanRoomAreas(Code);
GO

CREATE TABLE erp.CleanRoomEnvironmentalReadings (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CleanRoomEnvironmentalReadings PRIMARY KEY DEFAULT NEWID(),
    CleanRoomAreaId UNIQUEIDENTIFIER NOT NULL,
    RecordedAt DATETIMEOFFSET NOT NULL,
    TemperatureC DECIMAL(18,4) NOT NULL,
    HumidityPercent DECIMAL(18,4) NOT NULL,
    ParticleCountPerCubicFoot INT NOT NULL,
    SensorId NVARCHAR(80) NOT NULL,
    IsCompliant BIT NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_CleanRoomReadings_Areas FOREIGN KEY(CleanRoomAreaId) REFERENCES erp.CleanRoomAreas(Id)
);
CREATE INDEX IX_CleanRoomReadings_AreaTime ON erp.CleanRoomEnvironmentalReadings(CleanRoomAreaId, RecordedAt DESC);
GO

CREATE TABLE erp.CleanRoomAccessLogs (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CleanRoomAccessLogs PRIMARY KEY DEFAULT NEWID(),
    CleanRoomAreaId UNIQUEIDENTIFIER NOT NULL,
    UserId NVARCHAR(128) NOT NULL,
    EntryAt DATETIMEOFFSET NOT NULL,
    ExitAt DATETIMEOFFSET NULL,
    GowningChecklistPassed BIT NOT NULL,
    AccessApproved BIT NOT NULL,
    DenialReason NVARCHAR(512) NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_CleanRoomAccessLogs_Areas FOREIGN KEY(CleanRoomAreaId) REFERENCES erp.CleanRoomAreas(Id)
);
GO

CREATE TABLE erp.EnvironmentalComplianceRecords (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EnvironmentalComplianceRecords PRIMARY KEY DEFAULT NEWID(),
    CleanRoomAreaId UNIQUEIDENTIFIER NOT NULL,
    ComplianceDate DATE NOT NULL,
    TemperatureCompliant BIT NOT NULL,
    HumidityCompliant BIT NOT NULL,
    ParticleCompliant BIT NOT NULL,
    ReviewedByUserId NVARCHAR(128) NOT NULL,
    CorrectiveAction NVARCHAR(2000) NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_ComplianceRecords_Areas FOREIGN KEY(CleanRoomAreaId) REFERENCES erp.CleanRoomAreas(Id)
);
CREATE UNIQUE INDEX UX_Compliance_AreaDate ON erp.EnvironmentalComplianceRecords(CleanRoomAreaId, ComplianceDate);
GO

CREATE TABLE erp.Warehouses (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Warehouses PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(32) NOT NULL,
    Name NVARCHAR(160) NOT NULL,
    Description NVARCHAR(512) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    SiteCode NVARCHAR(40) NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL
);
CREATE UNIQUE INDEX UX_Warehouses_Code ON erp.Warehouses(Code);
GO

CREATE TABLE erp.WarehouseLocations (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WarehouseLocations PRIMARY KEY DEFAULT NEWID(),
    WarehouseId UNIQUEIDENTIFIER NOT NULL,
    Code NVARCHAR(32) NOT NULL,
    Name NVARCHAR(160) NOT NULL,
    Description NVARCHAR(512) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    Zone NVARCHAR(40) NOT NULL,
    Bin NVARCHAR(40) NOT NULL,
    IsCleanRoomStorage BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_WarehouseLocations_Warehouses FOREIGN KEY(WarehouseId) REFERENCES erp.Warehouses(Id)
);
CREATE UNIQUE INDEX UX_WarehouseLocations_Code ON erp.WarehouseLocations(Code);
GO

CREATE TABLE erp.InventoryItems (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_InventoryItems PRIMARY KEY DEFAULT NEWID(),
    ItemNumber NVARCHAR(80) NOT NULL,
    ItemType INT NOT NULL,
    ProductId UNIQUEIDENTIFIER NULL,
    WarehouseLocationId UNIQUEIDENTIFIER NOT NULL,
    BatchNumber NVARCHAR(64) NOT NULL,
    LotNumber NVARCHAR(64) NOT NULL,
    QuantityOnHand DECIMAL(18,4) NOT NULL DEFAULT 0,
    ReorderLevel DECIMAL(18,4) NOT NULL DEFAULT 0,
    StandardCost DECIMAL(18,4) NOT NULL DEFAULT 0,
    Uom NVARCHAR(16) NOT NULL,
    BarcodeValue NVARCHAR(256) NULL,
    QrCodeValue NVARCHAR(1024) NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_InventoryItems_Products FOREIGN KEY(ProductId) REFERENCES erp.Products(Id),
    CONSTRAINT FK_InventoryItems_Locations FOREIGN KEY(WarehouseLocationId) REFERENCES erp.WarehouseLocations(Id)
);
CREATE UNIQUE INDEX UX_InventoryItems_ItemNumber ON erp.InventoryItems(ItemNumber);
CREATE INDEX IX_InventoryItems_BatchLot ON erp.InventoryItems(BatchNumber, LotNumber);
GO

CREATE TABLE erp.InventoryTransactions (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_InventoryTransactions PRIMARY KEY DEFAULT NEWID(),
    TransactionNumber NVARCHAR(80) NOT NULL,
    InventoryItemId UNIQUEIDENTIFIER NOT NULL,
    TransactionType INT NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL,
    FromLocationId UNIQUEIDENTIFIER NULL,
    ToLocationId UNIQUEIDENTIFIER NULL,
    ReferenceNumber NVARCHAR(120) NULL,
    TransactionAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_InventoryTransactions_Items FOREIGN KEY(InventoryItemId) REFERENCES erp.InventoryItems(Id),
    CONSTRAINT FK_InventoryTransactions_FromLocation FOREIGN KEY(FromLocationId) REFERENCES erp.WarehouseLocations(Id),
    CONSTRAINT FK_InventoryTransactions_ToLocation FOREIGN KEY(ToLocationId) REFERENCES erp.WarehouseLocations(Id)
);
CREATE UNIQUE INDEX UX_InventoryTransactions_Number ON erp.InventoryTransactions(TransactionNumber);
GO

CREATE TABLE erp.VacuumPackagingRecords (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_VacuumPackagingRecords PRIMARY KEY DEFAULT NEWID(),
    PackagingBatchNumber NVARCHAR(80) NOT NULL,
    WorkOrderId UNIQUEIDENTIFIER NOT NULL,
    PackagingOperatorUserId NVARCHAR(128) NOT NULL,
    PackagingDate DATETIMEOFFSET NOT NULL,
    VacuumPressureKpa DECIMAL(18,4) NOT NULL,
    VacuumSealValidated BIT NOT NULL,
    PackagingInspectionPassed BIT NOT NULL,
    Remarks NVARCHAR(1000) NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_VacuumPackagingRecords_WorkOrders FOREIGN KEY(WorkOrderId) REFERENCES erp.WorkOrders(Id)
);
CREATE UNIQUE INDEX UX_VacuumPackaging_Batch ON erp.VacuumPackagingRecords(PackagingBatchNumber);
GO

CREATE TABLE erp.ShippingInspections (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ShippingInspections PRIMARY KEY DEFAULT NEWID(),
    ShippingInspectionNumber NVARCHAR(80) NOT NULL,
    VacuumPackagingRecordId UNIQUEIDENTIFIER NOT NULL,
    InspectionRecordId UNIQUEIDENTIFIER NULL,
    DispatchApproved BIT NOT NULL DEFAULT 0,
    ApprovedByUserId NVARCHAR(128) NULL,
    ApprovedAt DATETIMEOFFSET NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_ShippingInspections_Packaging FOREIGN KEY(VacuumPackagingRecordId) REFERENCES erp.VacuumPackagingRecords(Id),
    CONSTRAINT FK_ShippingInspections_InspectionRecords FOREIGN KEY(InspectionRecordId) REFERENCES erp.InspectionRecords(Id)
);
CREATE UNIQUE INDEX UX_ShippingInspections_Number ON erp.ShippingInspections(ShippingInspectionNumber);
GO

CREATE TABLE erp.Shipments (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Shipments PRIMARY KEY DEFAULT NEWID(),
    ShipmentNumber NVARCHAR(80) NOT NULL,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    Status INT NOT NULL,
    Carrier NVARCHAR(120) NULL,
    TrackingNumber NVARCHAR(120) NULL,
    ExportDocumentNumber NVARCHAR(120) NULL,
    DispatchedAt DATETIMEOFFSET NULL,
    DeliveredAt DATETIMEOFFSET NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_Shipments_Customers FOREIGN KEY(CustomerId) REFERENCES erp.Customers(Id)
);
CREATE UNIQUE INDEX UX_Shipments_Number ON erp.Shipments(ShipmentNumber);
GO

CREATE TABLE erp.ShipmentLines (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ShipmentLines PRIMARY KEY DEFAULT NEWID(),
    ShipmentId UNIQUEIDENTIFIER NOT NULL,
    VacuumPackagingRecordId UNIQUEIDENTIFIER NOT NULL,
    ProductSerialNumber NVARCHAR(120) NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_ShipmentLines_Shipments FOREIGN KEY(ShipmentId) REFERENCES erp.Shipments(Id),
    CONSTRAINT FK_ShipmentLines_Packaging FOREIGN KEY(VacuumPackagingRecordId) REFERENCES erp.VacuumPackagingRecords(Id)
);
GO

CREATE TABLE erp.ProductSerials (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ProductSerials PRIMARY KEY DEFAULT NEWID(),
    SerialNumber NVARCHAR(120) NOT NULL,
    WorkOrderId UNIQUEIDENTIFIER NOT NULL,
    BatchNumber NVARCHAR(64) NOT NULL,
    LotNumber NVARCHAR(64) NOT NULL,
    ManufacturingDate DATE NOT NULL,
    CurrentStatus NVARCHAR(80) NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_ProductSerials_WorkOrders FOREIGN KEY(WorkOrderId) REFERENCES erp.WorkOrders(Id)
);
CREATE UNIQUE INDEX UX_ProductSerials_SerialNumber ON erp.ProductSerials(SerialNumber);
CREATE INDEX IX_ProductSerials_BatchLot ON erp.ProductSerials(BatchNumber, LotNumber);
GO

CREATE TABLE erp.ProductGenealogyLinks (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ProductGenealogyLinks PRIMARY KEY DEFAULT NEWID(),
    ParentProductSerialId UNIQUEIDENTIFIER NOT NULL,
    ChildProductSerialId UNIQUEIDENTIFIER NOT NULL,
    RelationshipType NVARCHAR(80) NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_Genealogy_Parent FOREIGN KEY(ParentProductSerialId) REFERENCES erp.ProductSerials(Id),
    CONSTRAINT FK_Genealogy_Child FOREIGN KEY(ChildProductSerialId) REFERENCES erp.ProductSerials(Id),
    CONSTRAINT CK_Genealogy_NoSelf CHECK (ParentProductSerialId <> ChildProductSerialId)
);
CREATE UNIQUE INDEX UX_Genealogy_Link ON erp.ProductGenealogyLinks(ParentProductSerialId, ChildProductSerialId, RelationshipType);
GO

CREATE TABLE erp.TraceabilityEvents (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TraceabilityEvents PRIMARY KEY DEFAULT NEWID(),
    ProductSerialId UNIQUEIDENTIFIER NOT NULL,
    ManufacturingOperationId UNIQUEIDENTIFIER NULL,
    InspectionRecordId UNIQUEIDENTIFIER NULL,
    VacuumPackagingRecordId UNIQUEIDENTIFIER NULL,
    ShipmentId UNIQUEIDENTIFIER NULL,
    EventType NVARCHAR(80) NOT NULL,
    EventReference NVARCHAR(120) NOT NULL,
    EventAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_TraceabilityEvents_ProductSerials FOREIGN KEY(ProductSerialId) REFERENCES erp.ProductSerials(Id),
    CONSTRAINT FK_TraceabilityEvents_Operations FOREIGN KEY(ManufacturingOperationId) REFERENCES erp.ManufacturingOperations(Id),
    CONSTRAINT FK_TraceabilityEvents_InspectionRecords FOREIGN KEY(InspectionRecordId) REFERENCES erp.InspectionRecords(Id),
    CONSTRAINT FK_TraceabilityEvents_Packaging FOREIGN KEY(VacuumPackagingRecordId) REFERENCES erp.VacuumPackagingRecords(Id),
    CONSTRAINT FK_TraceabilityEvents_Shipments FOREIGN KEY(ShipmentId) REFERENCES erp.Shipments(Id)
);
CREATE INDEX IX_TraceabilityEvents_SerialTime ON erp.TraceabilityEvents(ProductSerialId, EventAt);
GO

CREATE TABLE erp.ManagedDocuments (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ManagedDocuments PRIMARY KEY DEFAULT NEWID(),
    DocumentNumber NVARCHAR(80) NOT NULL,
    Title NVARCHAR(240) NOT NULL,
    DocumentType INT NOT NULL,
    Revision NVARCHAR(20) NOT NULL,
    FileName NVARCHAR(260) NOT NULL,
    StorageUri NVARCHAR(1024) NOT NULL,
    OwnerDepartment NVARCHAR(120) NOT NULL,
    EffectiveAt DATETIMEOFFSET NOT NULL,
    ExpiresAt DATETIMEOFFSET NULL,
    IsReleased BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL
);
CREATE UNIQUE INDEX UX_ManagedDocuments_NumberRevision ON erp.ManagedDocuments(DocumentNumber, Revision);
GO

CREATE TABLE erp.AuditLogs (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY DEFAULT NEWID(),
    EntityName NVARCHAR(160) NOT NULL,
    EntityId NVARCHAR(80) NOT NULL,
    Action NVARCHAR(40) NOT NULL,
    ChangedBy NVARCHAR(128) NOT NULL,
    ChangedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    OldValuesJson NVARCHAR(MAX) NULL,
    NewValuesJson NVARCHAR(MAX) NULL,
    CorrelationId NVARCHAR(80) NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL
);
CREATE INDEX IX_AuditLogs_Entity ON erp.AuditLogs(EntityName, EntityId, ChangedAt DESC);
GO

CREATE TABLE erp.UserActivityAudits (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_UserActivityAudits PRIMARY KEY DEFAULT NEWID(),
    UserId NVARCHAR(128) NOT NULL,
    UserName NVARCHAR(256) NOT NULL,
    Activity NVARCHAR(512) NOT NULL,
    IpAddress NVARCHAR(80) NULL,
    UserAgent NVARCHAR(512) NULL,
    ActivityAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL DEFAULT 'system',
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL
);
CREATE INDEX IX_UserActivityAudits_UserTime ON erp.UserActivityAudits(UserId, ActivityAt DESC);
GO
