CREATE TABLE auth.ApplicationUsers (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ApplicationUsers PRIMARY KEY,
    UserName NVARCHAR(128) NOT NULL CONSTRAINT UQ_ApplicationUsers_UserName UNIQUE,
    Email NVARCHAR(256) NOT NULL CONSTRAINT UQ_ApplicationUsers_Email UNIQUE,
    PasswordHash NVARCHAR(512) NOT NULL,
    Role INT NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_ApplicationUsers_IsActive DEFAULT 1,
    LastLoginAtUtc DATETIME2 NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ApplicationUsers_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_ApplicationUsers_IsDeleted DEFAULT 0
);
GO

CREATE TABLE auth.RefreshTokens (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RefreshTokens PRIMARY KEY,
    ApplicationUserId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_RefreshTokens_User REFERENCES auth.ApplicationUsers(Id),
    TokenHash NVARCHAR(256) NOT NULL,
    ExpiresAtUtc DATETIME2 NOT NULL,
    RevokedAtUtc DATETIME2 NULL,
    ReplacedByTokenHash NVARCHAR(256) NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_RefreshTokens_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_RefreshTokens_IsDeleted DEFAULT 0
);
CREATE INDEX IX_RefreshTokens_UserExpiry ON auth.RefreshTokens(ApplicationUserId, ExpiresAtUtc DESC);
GO

CREATE TABLE auth.UserActivityAudits (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_UserActivityAudits PRIMARY KEY,
    ApplicationUserId UNIQUEIDENTIFIER NULL CONSTRAINT FK_UserActivityAudits_User REFERENCES auth.ApplicationUsers(Id),
    Action NVARCHAR(128) NOT NULL,
    EntityName NVARCHAR(128) NOT NULL,
    EntityId UNIQUEIDENTIFIER NULL,
    IpAddress NVARCHAR(64) NULL,
    UserAgent NVARCHAR(512) NULL,
    DetailsJson NVARCHAR(MAX) NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_UserActivityAudits_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_UserActivityAudits_IsDeleted DEFAULT 0
);
CREATE INDEX IX_UserActivityAudits_Entity ON auth.UserActivityAudits(EntityName, EntityId);
GO

CREATE TABLE erp.CustomerContacts (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerContacts PRIMARY KEY,
    CustomerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_CustomerContacts_Customer REFERENCES erp.Customers(Id),
    FullName NVARCHAR(160) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    Phone NVARCHAR(64) NULL,
    JobTitle NVARCHAR(128) NULL,
    IsPrimary BIT NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_CustomerContacts_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_CustomerContacts_IsDeleted DEFAULT 0
);
GO

CREATE TABLE erp.CustomerAddresses (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerAddresses PRIMARY KEY,
    CustomerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_CustomerAddresses_Customer REFERENCES erp.Customers(Id),
    AddressType NVARCHAR(32) NOT NULL,
    Line1 NVARCHAR(256) NOT NULL,
    Line2 NVARCHAR(256) NULL,
    City NVARCHAR(128) NOT NULL,
    State NVARCHAR(128) NOT NULL,
    Country NVARCHAR(128) NOT NULL,
    PostalCode NVARCHAR(32) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_CustomerAddresses_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_CustomerAddresses_IsDeleted DEFAULT 0
);
GO

CREATE TABLE erp.CustomerIndustries (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerIndustries PRIMARY KEY,
    CustomerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_CustomerIndustries_Customer REFERENCES erp.Customers(Id),
    IndustrySegment INT NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_CustomerIndustries_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_CustomerIndustries_IsDeleted DEFAULT 0,
    CONSTRAINT UQ_CustomerIndustries UNIQUE(CustomerId, IndustrySegment)
);
GO

CREATE TABLE erp.CustomerDocuments (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerDocuments PRIMARY KEY,
    CustomerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_CustomerDocuments_Customer REFERENCES erp.Customers(Id),
    DocumentType NVARCHAR(64) NOT NULL,
    FileName NVARCHAR(256) NOT NULL,
    StorageUri NVARCHAR(1024) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_CustomerDocuments_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_CustomerDocuments_IsDeleted DEFAULT 0
);
GO

CREATE TABLE erp.ProductRevisions (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ProductRevisions PRIMARY KEY,
    ProductId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_ProductRevisions_Product REFERENCES erp.Products(Id),
    RevisionNumber NVARCHAR(32) NOT NULL,
    EngineeringDrawingNumber NVARCHAR(128) NOT NULL,
    DrawingStorageUri NVARCHAR(1024) NOT NULL,
    EffectiveFromUtc DATETIME2 NOT NULL,
    EffectiveToUtc DATETIME2 NULL,
    ChangeSummary NVARCHAR(1000) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ProductRevisions_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_ProductRevisions_IsDeleted DEFAULT 0,
    CONSTRAINT UQ_ProductRevisions_ProductRevision UNIQUE(ProductId, RevisionNumber)
);
GO

CREATE TABLE erp.EngineeringChangeRequests (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EngineeringChangeRequests PRIMARY KEY,
    ProductRevisionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_ECR_ProductRevision REFERENCES erp.ProductRevisions(Id),
    RequestNumber NVARCHAR(64) NOT NULL CONSTRAINT UQ_ECR_RequestNumber UNIQUE,
    Reason NVARCHAR(1000) NOT NULL,
    ApprovalStatus INT NOT NULL,
    ApprovedBy NVARCHAR(128) NULL,
    ApprovedAtUtc DATETIME2 NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ECR_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_ECR_IsDeleted DEFAULT 0
);
GO

CREATE TABLE sales.RequestForQuotations (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RequestForQuotations PRIMARY KEY,
    RfqNumber NVARCHAR(64) NOT NULL CONSTRAINT UQ_RFQ_Number UNIQUE,
    CustomerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_RFQ_Customer REFERENCES erp.Customers(Id),
    RequestedDateUtc DATETIME2 NOT NULL,
    Status INT NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_RFQ_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_RFQ_IsDeleted DEFAULT 0
);
GO

CREATE TABLE sales.RequestForQuotationLines (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RequestForQuotationLines PRIMARY KEY,
    RequestForQuotationId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_RFQLines_RFQ REFERENCES sales.RequestForQuotations(Id),
    ProductId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_RFQLines_Product REFERENCES erp.Products(Id),
    Quantity INT NOT NULL CONSTRAINT CK_RFQLines_Qty CHECK(Quantity > 0),
    RequiredDateUtc DATETIME2 NOT NULL,
    Notes NVARCHAR(1000) NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_RFQLines_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_RFQLines_IsDeleted DEFAULT 0
);
GO

CREATE TABLE sales.Quotations (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Quotations PRIMARY KEY,
    QuotationNumber NVARCHAR(64) NOT NULL CONSTRAINT UQ_Quotations_Number UNIQUE,
    RequestForQuotationId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Quotations_RFQ REFERENCES sales.RequestForQuotations(Id),
    ApprovalStatus INT NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Currency CHAR(3) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Quotations_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Quotations_IsDeleted DEFAULT 0
);
GO

CREATE TABLE sales.QuotationLines (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_QuotationLines PRIMARY KEY,
    QuotationId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_QuotationLines_Quotation REFERENCES sales.Quotations(Id),
    ProductId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_QuotationLines_Product REFERENCES erp.Products(Id),
    Quantity INT NOT NULL CONSTRAINT CK_QuotationLines_Qty CHECK(Quantity > 0),
    UnitPrice DECIMAL(18,2) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_QuotationLines_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_QuotationLines_IsDeleted DEFAULT 0
);
GO

CREATE TABLE sales.SalesOrders (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SalesOrders PRIMARY KEY,
    SalesOrderNumber NVARCHAR(64) NOT NULL CONSTRAINT UQ_SalesOrders_Number UNIQUE,
    CustomerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_SalesOrders_Customer REFERENCES erp.Customers(Id),
    QuotationId UNIQUEIDENTIFIER NULL CONSTRAINT FK_SalesOrders_Quotation REFERENCES sales.Quotations(Id),
    Status INT NOT NULL,
    OrderDateUtc DATETIME2 NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_SalesOrders_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_SalesOrders_IsDeleted DEFAULT 0
);
GO

CREATE TABLE sales.SalesOrderLines (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SalesOrderLines PRIMARY KEY,
    SalesOrderId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_SalesOrderLines_Order REFERENCES sales.SalesOrders(Id),
    ProductId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_SalesOrderLines_Product REFERENCES erp.Products(Id),
    Quantity INT NOT NULL CONSTRAINT CK_SalesOrderLines_Qty CHECK(Quantity > 0),
    PromiseDateUtc DATETIME2 NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_SalesOrderLines_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_SalesOrderLines_IsDeleted DEFAULT 0
);
GO

CREATE TABLE mes.Shifts (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Shifts PRIMARY KEY,
    Name NVARCHAR(64) NOT NULL,
    StartsAt TIME NOT NULL,
    EndsAt TIME NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Shifts_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Shifts_IsDeleted DEFAULT 0
);
GO

CREATE TABLE mes.ProductionSchedules (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ProductionSchedules PRIMARY KEY,
    ScheduleNumber NVARCHAR(64) NOT NULL CONSTRAINT UQ_ProductionSchedules_Number UNIQUE,
    ScheduleDate DATE NOT NULL,
    ShiftId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_ProductionSchedules_Shift REFERENCES mes.Shifts(Id),
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ProductionSchedules_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_ProductionSchedules_IsDeleted DEFAULT 0
);
ALTER TABLE mes.WorkOrders ADD SalesOrderLineId UNIQUEIDENTIFIER NULL CONSTRAINT FK_WorkOrders_SalesOrderLine REFERENCES sales.SalesOrderLines(Id);
ALTER TABLE mes.WorkOrders ADD ProductionScheduleId UNIQUEIDENTIFIER NULL CONSTRAINT FK_WorkOrders_ProductionSchedule REFERENCES mes.ProductionSchedules(Id);
GO

CREATE TABLE qms.InspectionMeasurements (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_InspectionMeasurements PRIMARY KEY,
    InspectionRecordId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_InspectionMeasurements_Record REFERENCES qms.InspectionRecords(Id),
    ParameterName NVARCHAR(128) NOT NULL,
    Specification NVARCHAR(256) NOT NULL,
    MeasuredValue NVARCHAR(256) NOT NULL,
    IsPass BIT NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_InspectionMeasurements_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_InspectionMeasurements_IsDeleted DEFAULT 0
);
GO

CREATE TABLE qms.DefectRecords (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DefectRecords PRIMARY KEY,
    InspectionRecordId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_DefectRecords_Record REFERENCES qms.InspectionRecords(Id),
    DefectCode NVARCHAR(64) NOT NULL,
    Description NVARCHAR(1000) NOT NULL,
    Severity NVARCHAR(32) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_DefectRecords_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_DefectRecords_IsDeleted DEFAULT 0
);
GO

CREATE TABLE qms.NonConformanceReports (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_NonConformanceReports PRIMARY KEY,
    NcrNumber NVARCHAR(64) NOT NULL CONSTRAINT UQ_NCR_Number UNIQUE,
    WorkOrderId UNIQUEIDENTIFIER NULL CONSTRAINT FK_NCR_WorkOrder REFERENCES mes.WorkOrders(Id),
    ProblemStatement NVARCHAR(2000) NOT NULL,
    Disposition INT NOT NULL,
    Status NVARCHAR(32) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_NCR_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_NCR_IsDeleted DEFAULT 0
);
GO

CREATE TABLE qms.CapaRecords (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CapaRecords PRIMARY KEY,
    CapaNumber NVARCHAR(64) NOT NULL CONSTRAINT UQ_CAPA_Number UNIQUE,
    NonConformanceReportId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_CAPA_NCR REFERENCES qms.NonConformanceReports(Id),
    RootCause NVARCHAR(2000) NOT NULL,
    CorrectiveAction NVARCHAR(2000) NOT NULL,
    PreventiveAction NVARCHAR(2000) NOT NULL,
    Status NVARCHAR(32) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_CAPA_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_CAPA_IsDeleted DEFAULT 0
);
GO

CREATE TABLE whs.Items (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Items PRIMARY KEY,
    ItemCode NVARCHAR(64) NOT NULL CONSTRAINT UQ_Items_Code UNIQUE,
    Name NVARCHAR(256) NOT NULL,
    ItemType NVARCHAR(64) NOT NULL,
    UnitOfMeasure NVARCHAR(16) NOT NULL,
    ReorderLevel DECIMAL(18,4) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Items_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Items_IsDeleted DEFAULT 0
);
GO

CREATE TABLE whs.Warehouses (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Warehouses PRIMARY KEY,
    WarehouseCode NVARCHAR(32) NOT NULL CONSTRAINT UQ_Warehouses_Code UNIQUE,
    Name NVARCHAR(128) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Warehouses_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Warehouses_IsDeleted DEFAULT 0
);
GO

CREATE TABLE whs.WarehouseLocations (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WarehouseLocations PRIMARY KEY,
    WarehouseId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_WarehouseLocations_Warehouse REFERENCES whs.Warehouses(Id),
    LocationCode NVARCHAR(64) NOT NULL,
    CleanRoomClass NVARCHAR(32) NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_WarehouseLocations_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_WarehouseLocations_IsDeleted DEFAULT 0,
    CONSTRAINT UQ_WarehouseLocations UNIQUE(WarehouseId, LocationCode)
);
GO

CREATE TABLE whs.Bins (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Bins PRIMARY KEY,
    WarehouseLocationId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Bins_Location REFERENCES whs.WarehouseLocations(Id),
    BinCode NVARCHAR(64) NOT NULL,
    BarcodeValue NVARCHAR(256) NULL,
    QrCodeValue NVARCHAR(256) NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Bins_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Bins_IsDeleted DEFAULT 0,
    CONSTRAINT UQ_Bins UNIQUE(WarehouseLocationId, BinCode)
);
GO

CREATE TABLE whs.InventoryStocks (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_InventoryStocks PRIMARY KEY,
    ItemId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_InventoryStocks_Item REFERENCES whs.Items(Id),
    BinId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_InventoryStocks_Bin REFERENCES whs.Bins(Id),
    LotNumber NVARCHAR(64) NOT NULL,
    QuantityOnHand DECIMAL(18,4) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_InventoryStocks_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_InventoryStocks_IsDeleted DEFAULT 0,
    CONSTRAINT UQ_InventoryStocks_ItemBinLot UNIQUE(ItemId, BinId, LotNumber)
);
GO

CREATE TABLE whs.InventoryTransactions (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_InventoryTransactions PRIMARY KEY,
    TransactionNumber NVARCHAR(64) NOT NULL CONSTRAINT UQ_InventoryTransactions_Number UNIQUE,
    ItemId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_InventoryTransactions_Item REFERENCES whs.Items(Id),
    FromBinId UNIQUEIDENTIFIER NULL CONSTRAINT FK_InventoryTransactions_FromBin REFERENCES whs.Bins(Id),
    ToBinId UNIQUEIDENTIFIER NULL CONSTRAINT FK_InventoryTransactions_ToBin REFERENCES whs.Bins(Id),
    TransactionType INT NOT NULL,
    LotNumber NVARCHAR(64) NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL,
    ReferenceNumber NVARCHAR(128) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_InventoryTransactions_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_InventoryTransactions_IsDeleted DEFAULT 0
);
CREATE INDEX IX_InventoryTransactions_ItemLot ON whs.InventoryTransactions(ItemId, LotNumber);
GO

CREATE TABLE cleanroom.CleanRoomAccessLogs (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CleanRoomAccessLogs PRIMARY KEY,
    CleanRoomId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_CleanRoomAccessLogs_CleanRoom REFERENCES cleanroom.CleanRooms(Id),
    ApplicationUserId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_CleanRoomAccessLogs_User REFERENCES auth.ApplicationUsers(Id),
    EntryTimeUtc DATETIME2 NOT NULL,
    ExitTimeUtc DATETIME2 NULL,
    GowningChecklistStatus NVARCHAR(32) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_CleanRoomAccessLogs_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_CleanRoomAccessLogs_IsDeleted DEFAULT 0
);
CREATE INDEX IX_CleanRoomAccessLogs_RoomEntry ON cleanroom.CleanRoomAccessLogs(CleanRoomId, EntryTimeUtc DESC);
GO

CREATE TABLE shipping.PackagingBatches (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PackagingBatches PRIMARY KEY,
    PackagingBatchNumber NVARCHAR(64) NOT NULL CONSTRAINT UQ_PackagingBatches_Number UNIQUE,
    WorkOrderId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_PackagingBatches_WorkOrder REFERENCES mes.WorkOrders(Id),
    PackagingOperator NVARCHAR(128) NOT NULL,
    PackagingDateUtc DATETIME2 NOT NULL,
    VacuumSealValidated BIT NOT NULL,
    PackagingInspectionResult NVARCHAR(64) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_PackagingBatches_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_PackagingBatches_IsDeleted DEFAULT 0
);
GO

CREATE TABLE shipping.Shipments (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Shipments PRIMARY KEY,
    ShipmentNumber NVARCHAR(64) NOT NULL CONSTRAINT UQ_Shipments_Number UNIQUE,
    CustomerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Shipments_Customer REFERENCES erp.Customers(Id),
    PlannedShipDateUtc DATETIME2 NOT NULL,
    DispatchApprovalStatus NVARCHAR(64) NOT NULL,
    ShipmentStatus NVARCHAR(64) NOT NULL,
    TrackingNumber NVARCHAR(128) NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Shipments_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Shipments_IsDeleted DEFAULT 0
);
GO

CREATE TABLE shipping.ShipmentLines (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ShipmentLines PRIMARY KEY,
    ShipmentId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_ShipmentLines_Shipment REFERENCES shipping.Shipments(Id),
    ProductSerialId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_ShipmentLines_ProductSerial REFERENCES mes.ProductSerials(Id),
    PackagingBatchId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_ShipmentLines_PackagingBatch REFERENCES shipping.PackagingBatches(Id),
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ShipmentLines_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_ShipmentLines_IsDeleted DEFAULT 0,
    CONSTRAINT UQ_ShipmentLines_ProductSerial UNIQUE(ProductSerialId)
);
GO

CREATE TABLE docs.ManagedDocuments (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ManagedDocuments PRIMARY KEY,
    DocumentNumber NVARCHAR(64) NOT NULL,
    DocumentType NVARCHAR(64) NOT NULL,
    Title NVARCHAR(256) NOT NULL,
    Revision NVARCHAR(32) NOT NULL,
    StorageUri NVARCHAR(1024) NOT NULL,
    EffectiveDateUtc DATETIME2 NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_ManagedDocuments_Created DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(128) NOT NULL,
    ModifiedAtUtc DATETIME2 NULL,
    ModifiedBy NVARCHAR(128) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_ManagedDocuments_IsDeleted DEFAULT 0,
    CONSTRAINT UQ_ManagedDocuments_NumberRevision UNIQUE(DocumentNumber, Revision)
);
GO
