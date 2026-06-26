using AWQP.Domain.Enums;

namespace AWQP.Application.DTOs;

public sealed record CustomerDto(Guid Id, string CustomerCode, string Name, string? Website, IReadOnlyCollection<string> Industries);
public sealed record CreateCustomerRequest(string CustomerCode, string Name, string? TaxNumber, string? Website, IReadOnlyCollection<IndustrySegment> Industries);

public sealed record ProductDto(Guid Id, string ProductCode, string Name, string Category, string MaterialGrade, decimal OuterDiameterMm, decimal InnerDiameterMm, decimal LengthMm, decimal WallThicknessMm);
public sealed record CreateProductCategoryRequest(string Name, IndustrySegment IndustrySegment);
public sealed record CreateProductRequest(string ProductCode, string Name, Guid ProductCategoryId, string MaterialGrade, decimal OuterDiameterMm, decimal InnerDiameterMm, decimal LengthMm, decimal WallThicknessMm);
public sealed record CreateProductRevisionRequest(Guid ProductId, string RevisionNumber, string EngineeringDrawingNumber, string DrawingStorageUri, string ChangeSummary);

public sealed record CreateRfqLineRequest(Guid ProductId, int Quantity, DateTime RequiredDateUtc, string? Notes);
public sealed record CreateRfqRequest(string RfqNumber, Guid CustomerId, IReadOnlyCollection<CreateRfqLineRequest> Lines);
public sealed record CreateQuotationLineRequest(Guid ProductId, int Quantity, decimal UnitPrice);
public sealed record CreateQuotationRequest(string QuotationNumber, Guid RequestForQuotationId, string Currency, IReadOnlyCollection<CreateQuotationLineRequest> Lines);
public sealed record CreateSalesOrderLineRequest(Guid ProductId, int Quantity, DateTime PromiseDateUtc);
public sealed record CreateSalesOrderRequest(string SalesOrderNumber, Guid CustomerId, Guid? QuotationId, IReadOnlyCollection<CreateSalesOrderLineRequest> Lines);

public sealed record WorkOrderDto(Guid Id, string WorkOrderNumber, string BatchNumber, string LotNumber, string ProductCode, int QuantityPlanned, int QuantityProduced, int QuantityRejected, WorkOrderStatus Status);
public sealed record CreateWorkOrderRequest(string WorkOrderNumber, string BatchNumber, string LotNumber, Guid ProductId, int QuantityPlanned);

public sealed record OperationStartRequest(Guid OperationId, string OperatorName, Guid? MachineId);
public sealed record OperationCompleteRequest(Guid OperationId, int QuantityProduced, int QuantityRejected, string? Remarks);

public sealed record InspectionRecordDto(Guid Id, string InspectionNumber, ProcessStageType InspectionStage, string InspectorName, QualityDisposition Disposition);
public sealed record CreateInspectionRequest(string InspectionNumber, Guid? WorkOrderId, Guid? ProductSerialId, ProcessStageType InspectionStage, string InspectorName, IReadOnlyCollection<CreateInspectionMeasurementRequest> Measurements);
public sealed record CreateInspectionMeasurementRequest(string ParameterName, string Specification, string MeasuredValue, bool IsPass);

public sealed record EnvironmentalReadingDto(Guid Id, string CleanRoomCode, DateTime ReadingTimeUtc, decimal TemperatureC, decimal HumidityPercent, int ParticleCount, ComplianceStatus ComplianceStatus);
public sealed record CreateEnvironmentalReadingRequest(Guid CleanRoomId, decimal TemperatureC, decimal HumidityPercent, int ParticleCount);

public sealed record CreateItemRequest(string ItemCode, string Name, string ItemType, string UnitOfMeasure, decimal ReorderLevel);
public sealed record CreateWarehouseRequest(string WarehouseCode, string Name);
public sealed record CreateWarehouseLocationRequest(Guid WarehouseId, string LocationCode, string? CleanRoomClass);
public sealed record CreateBinRequest(Guid WarehouseLocationId, string BinCode, string? BarcodeValue, string? QrCodeValue);
public sealed record CreateInventoryTransactionRequest(string TransactionNumber, Guid ItemId, Guid? FromBinId, Guid? ToBinId, InventoryTransactionType TransactionType, string LotNumber, decimal Quantity, string ReferenceNumber);

public sealed record CreatePackagingBatchRequest(string PackagingBatchNumber, Guid WorkOrderId, string PackagingOperator, bool VacuumSealValidated, string PackagingInspectionResult);
public sealed record CreateShipmentLineRequest(Guid ProductSerialId, Guid PackagingBatchId);
public sealed record CreateShipmentRequest(string ShipmentNumber, Guid CustomerId, DateTime PlannedShipDateUtc, string DispatchApprovalStatus, IReadOnlyCollection<CreateShipmentLineRequest> Lines);

public sealed record CreateManagedDocumentRequest(string DocumentNumber, string DocumentType, string Title, string Revision, string StorageUri, DateTime EffectiveDateUtc);

public sealed record TraceabilityDto(string SerialNumber, string WorkOrderNumber, string BatchNumber, string LotNumber, string ProductCode, DateTime ManufacturingDateUtc, IReadOnlyCollection<string> Operations, IReadOnlyCollection<string> Inspections, IReadOnlyCollection<string> PackagingBatches, IReadOnlyCollection<string> Shipments);

public sealed record DashboardMetricDto(string Name, decimal Value, string Unit, string Status);
public sealed record DashboardDto(IReadOnlyCollection<DashboardMetricDto> Production, IReadOnlyCollection<DashboardMetricDto> Quality, IReadOnlyCollection<DashboardMetricDto> Inventory, IReadOnlyCollection<DashboardMetricDto> CleanRoom, IReadOnlyCollection<DashboardMetricDto> Shipping);
