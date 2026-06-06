using AutoMapper;
using AWQP.Domain.CleanRoom;
using AWQP.Domain.Customers;
using AWQP.Domain.Inventory;
using AWQP.Domain.Logistics;
using AWQP.Domain.Production;
using AWQP.Domain.Products;
using AWQP.Domain.Quality;
using AWQP.Domain.Sales;

namespace AWQP.Application.Common;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Customer, CustomerDto>();
        CreateMap<CreateCustomerRequest, Customer>();
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.Segment, o => o.MapFrom(s => s.ProductCategory!.Segment))
            .ForMember(d => d.MaterialGrade, o => o.MapFrom(s => s.MaterialGrade!.Name));
        CreateMap<CreateProductRequest, Product>();
        CreateMap<RequestForQuote, RfqDto>();
        CreateMap<CreateRfqRequest, RequestForQuote>();
        CreateMap<SalesOrder, SalesOrderDto>();
        CreateMap<WorkOrder, WorkOrderDto>();
        CreateMap<ManufacturingOperation, ManufacturingOperationDto>();
        CreateMap<InspectionRecord, InspectionRecordDto>();
        CreateMap<CleanRoomEnvironmentalReading, CleanRoomReadingDto>()
            .ForMember(d => d.AreaId, o => o.MapFrom(s => s.CleanRoomAreaId));
        CreateMap<InventoryItem, InventoryItemDto>();
        CreateMap<VacuumPackagingRecord, VacuumPackagingDto>();
        CreateMap<Shipment, ShipmentDto>();
    }
}
