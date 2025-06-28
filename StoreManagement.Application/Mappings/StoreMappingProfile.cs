namespace StoreManagement.Application.Mappings;

public class StoreMappingProfile : Profile
{
    public StoreMappingProfile()
    {
        CreateMap<Store, StoreDto>()
            .ForMember(dest => dest.Phone_Number, opt => opt.MapFrom(src => src.Phone.Value))
            .ForMember(dest => dest.Address_City, opt => opt.MapFrom(src => src.Address.City))
            .ForMember(dest => dest.Address_FullAddress, opt => opt.MapFrom(src => src.Address.FullAddress));

    }
}