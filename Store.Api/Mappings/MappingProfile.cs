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

            // Mapeo para actualización
            CreateMap<ProductUpdateDto, Product>();

            // Dentro del constructor MappingProfile()
            CreateMap<VentaCreateDto, Venta>();
            CreateMap<VentaDetalleDto, VentaDetalle>();

            // Para las lecturas (Dapper -> DTO)
            CreateMap<Venta, VentaReadDto>();
            CreateMap<VentaDetalle, VentaDetalleReadDto>().ForMember(dest => dest.NombreProducto, opt =>opt.MapFrom(src => src.Product.Name));
        }
    }
}
