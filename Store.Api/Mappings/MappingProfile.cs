using AutoMapper;
using Store.Data.DTO;
using Store.Domain.Models;

namespace Store.Api.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // De Entidad a DTO
            CreateMap<Product, ProductResponseDto>().ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

            // De DTO a Entidad
            CreateMap<ProductCreateDto, Product>();
        }
    }
}
