namespace AWQP.Domain.Common;

public static class Roles
{
    public const string Admin = "Admin";
    public const string ProductionManager = "Production Manager";
    public const string ProductionEngineer = "Production Engineer";
    public const string QualityEngineer = "Quality Engineer";
    public const string CleanRoomOperator = "Clean Room Operator";
    public const string WarehouseStaff = "Warehouse Staff";
    public const string SalesTeam = "Sales Team";
    public const string Customer = "Customer";

    public static readonly string[] All =
    [
        Admin, ProductionManager, ProductionEngineer, QualityEngineer,
        CleanRoomOperator, WarehouseStaff, SalesTeam, Customer
    ];
}

public enum IndustrySegment { Semiconductor = 1, CompoundSemiconductor = 2, OpticalFiber = 3, SolarCell = 4 }
public enum SalesDocumentStatus { Draft = 1, Submitted = 2, Approved = 3, Rejected = 4, Converted = 5, Cancelled = 6 }
public enum WorkOrderStatus { Planned = 1, Released = 2, InProgress = 3, OnHold = 4, Completed = 5, Cancelled = 6 }
public enum OperationStage
{
    RawMaterialPreparation = 1,
    QuartzProcessing = 2,
    Forming = 3,
    FiringProcess = 4,
    Inspection = 5,
    CleanRoomEntry = 6,
    FinalCleaning = 7,
    VacuumPackaging = 8,
    ShippingInspection = 9,
    Dispatch = 10
}

public enum OperationStatus { Queued = 1, Running = 2, Paused = 3, Completed = 4, Failed = 5, Cancelled = 6 }
public enum InspectionType { Incoming = 1, InProcess = 2, Final = 3, Shipping = 4, Packaging = 5 }
public enum InspectionDisposition { Pending = 1, Accepted = 2, Rejected = 3, Conditional = 4 }
public enum Severity { Low = 1, Medium = 2, High = 3, Critical = 4 }
public enum CapaStatus { Open = 1, Investigating = 2, ActionPlanned = 3, Implemented = 4, Verified = 5, Closed = 6 }
public enum InventoryItemType { RawMaterial = 1, SemiFinishedGoods = 2, FinishedGoods = 3, Consumable = 4 }
public enum InventoryTransactionType { Receipt = 1, Issue = 2, Consumption = 3, Return = 4, Adjustment = 5, Transfer = 6 }
public enum ShipmentStatus { PendingInspection = 1, Approved = 2, Dispatched = 3, InTransit = 4, Delivered = 5, Closed = 6 }
public enum DocumentType { Sop = 1, Drawing = 2, InspectionReport = 3, PackagingReport = 4, Certificate = 5, Contract = 6 }
