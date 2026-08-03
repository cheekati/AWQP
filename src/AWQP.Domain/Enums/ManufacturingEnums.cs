namespace AWQP.Domain.Enums;

public enum UserRole
{
    Admin = 1,
    ProductionManager,
    ProductionEngineer,
    QualityEngineer,
    CleanRoomOperator,
    WarehouseStaff,
    SalesTeam,
    Customer
}

public enum IndustrySegment
{
    Semiconductor = 1,
    CompoundSemiconductor,
    OpticalFiber,
    SolarCell
}

public enum SalesStatus
{
    Draft = 1,
    Submitted,
    Approved,
    Rejected,
    ConvertedToOrder,
    Closed
}

public enum WorkOrderStatus
{
    Planned = 1,
    Released,
    InProgress,
    Hold,
    Completed,
    Cancelled
}

public enum ProcessStageType
{
    RawMaterialPreparation = 1,
    QuartzProcessing,
    Forming,
    FiringProcess,
    Inspection,
    CleanRoomEntry,
    FinalCleaning,
    VacuumPackaging,
    ShippingInspection,
    Dispatch
}

public enum QualityDisposition
{
    Accepted = 1,
    Rejected,
    Rework,
    UseAsIs,
    Scrap
}

public enum InventoryTransactionType
{
    Receipt = 1,
    Issue,
    Consumption,
    Return,
    Adjustment,
    Transfer
}

public enum ComplianceStatus
{
    Compliant = 1,
    Warning,
    NonCompliant,
    InvestigationRequired
}
