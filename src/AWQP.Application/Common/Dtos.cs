using AWQP.Domain.Common;

namespace AWQP.Application.Common;

public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshTokenRequest(string AccessToken, string RefreshToken);
public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt, string UserId, string UserName, IReadOnlyList<string> Roles);

public sealed record CustomerDto(Guid Id, string Code, string Name, string LegalName, string PaymentTerms, string CreditStatus);
public sealed record CreateCustomerRequest(string Code, string Name, string LegalName, string TaxNumber, string PaymentTerms);

public sealed record ProductDto(Guid Id, string Code, string Name, IndustrySegment Segment, string MaterialGrade, string ProductFamily, string DefaultUom);
public sealed record CreateProductRequest(string Code, string Name, Guid ProductCategoryId, Guid MaterialGradeId, string ProductFamily, string DefaultUom);

public sealed record RfqDto(Guid Id, string RfqNumber, Guid CustomerId, Guid ProductId, decimal Quantity, DateOnly RequiredDate, SalesDocumentStatus Status);
public sealed record CreateRfqRequest(Guid CustomerId, Guid ProductId, decimal Quantity, DateOnly RequiredDate, string TechnicalRequirements);

public sealed record SalesOrderDto(Guid Id, string SalesOrderNumber, Guid CustomerId, DateOnly RequestedShipDate, SalesDocumentStatus Status);

public sealed record WorkOrderDto(
    Guid Id,
    string WorkOrderNumber,
    Guid ProductId,
    string BatchNumber,
    string LotNumber,
    decimal QuantityPlanned,
    decimal QuantityProduced,
    decimal QuantityRejected,
    decimal YieldPercentage,
    WorkOrderStatus Status);

public sealed record CreateWorkOrderRequest(Guid SalesOrderLineId, Guid ProductId, string BatchNumber, string LotNumber, decimal QuantityPlanned);

public sealed record ManufacturingOperationDto(
    Guid Id,
    Guid WorkOrderId,
    OperationStage Stage,
    int Sequence,
    string OperatorUserId,
    Guid? MachineId,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    decimal QuantityPlanned,
    decimal QuantityProduced,
    decimal QuantityRejected,
    decimal YieldPercentage,
    OperationStatus Status,
    string? Remarks);

public sealed record CompleteOperationRequest(Guid OperationId, decimal QuantityProduced, decimal QuantityRejected, string? Remarks);

public sealed record InspectionRecordDto(Guid Id, string InspectionNumber, InspectionType InspectionType, Guid? WorkOrderId, InspectionDisposition Disposition, string InspectorUserId);
public sealed record CleanRoomReadingDto(Guid Id, Guid AreaId, DateTimeOffset RecordedAt, decimal TemperatureC, decimal HumidityPercent, int ParticleCountPerCubicFoot, bool IsCompliant);
public sealed record InventoryItemDto(Guid Id, string ItemNumber, InventoryItemType ItemType, string BatchNumber, string LotNumber, decimal QuantityOnHand, decimal ReorderLevel, decimal StandardCost);
public sealed record VacuumPackagingDto(Guid Id, string PackagingBatchNumber, Guid WorkOrderId, DateTimeOffset PackagingDate, bool VacuumSealValidated, bool PackagingInspectionPassed);
public sealed record ShipmentDto(Guid Id, string ShipmentNumber, Guid CustomerId, ShipmentStatus Status, string Carrier, string TrackingNumber);

public sealed record GenealogyEventDto(string EventType, string EventReference, DateTimeOffset EventAt, string? WorkOrderNumber, string? Stage);
public sealed record GenealogyNodeDto(string SerialNumber, string BatchNumber, string LotNumber, string Status, IReadOnlyList<GenealogyNodeDto> Children);
public sealed record GenealogyDto(string SerialNumber, string BatchNumber, string LotNumber, DateOnly ManufacturingDate, IReadOnlyList<GenealogyEventDto> Events, IReadOnlyList<GenealogyNodeDto> Children);

public sealed record ProductionDashboardDto(decimal DailyProduction, decimal MachineUtilizationPercent, decimal ProductionEfficiencyPercent, decimal YieldPercent);
public sealed record QualityDashboardDto(int NcrCount, int OpenCapaCount, IReadOnlyDictionary<string, int> DefectAnalysis);
public sealed record InventoryDashboardDto(decimal CurrentStock, decimal InventoryValuation, int ItemsBelowReorderLevel);
public sealed record CleanRoomDashboardDto(decimal AverageTemperatureC, decimal AverageHumidityPercent, int MaxParticleCount, bool ComplianceStatus);
public sealed record ShippingDashboardDto(int PendingDispatches, int DeliveriesToday, IReadOnlyDictionary<string, int> ShipmentStatus);
