using AutoMapper;
using ParserService.Data.Entities;
using ParserService.ParserCore.Models;

namespace ParserService.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Operator, OperatorEntity>().ReverseMap();
            CreateMap<Country, CountryEntity>().ReverseMap();
            CreateMap<Tour, TourEntity>().ReverseMap();
            CreateMap<Region, RegionEntity>().ReverseMap();
            CreateMap<Hotel, HotelEntity>().ReverseMap();

            CreateMap<Direction, DirectionEntity>().ReverseMap();
            CreateMap<Currency, CurrencyEntity>().ReverseMap();
            CreateMap<Placement, PlacementEntity>().ReverseMap();
            CreateMap<PriceType, PriceTypeEntity>().ReverseMap();

            CreateMap<Price, PriceEntity>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.PriceValue))
                .ReverseMap()
                .ForMember(dest => dest.PriceValue, opt => opt.MapFrom(src => src.Price));
        }
    }
}
