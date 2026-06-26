using AutoMapper;
using AWQP.Application.DTOs;
using AWQP.Domain.Entities;

namespace AWQP.Application.Mapping;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .ForCtorParam("Industries", opt => opt.MapFrom(src => src.Industries.Select(i => i.IndustrySegment.ToString()).ToArray()));
        CreateMap<Product, ProductDto>()
            .ForCtorParam("Category", opt => opt.MapFrom(src => src.ProductCategory.Name));
        CreateMap<WorkOrder, WorkOrderDto>()
            .ForCtorParam("ProductCode", opt => opt.MapFrom(src => src.Product.ProductCode));
        CreateMap<InspectionRecord, InspectionRecordDto>();
        CreateMap<EnvironmentalReading, EnvironmentalReadingDto>()
            .ForCtorParam("CleanRoomCode", opt => opt.MapFrom(src => src.CleanRoom.CleanRoomCode));
    }
}
