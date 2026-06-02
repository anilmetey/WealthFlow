using AutoMapper;
using WealthFlow.Application.DTOs;
using WealthFlow.Domain.Entities;

namespace WealthFlow.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Category Maps
            CreateMap<Category, CategoryDto>().ReverseMap();

            // Wallet Maps
            CreateMap<Wallet, WalletDto>().ReverseMap();

            // Transaction Maps (Include flattening mapping for Category and Wallet)
            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.CategoryColor, opt => opt.MapFrom(src => src.Category != null ? src.Category.Color : string.Empty))
                .ForMember(dest => dest.CategoryIcon, opt => opt.MapFrom(src => src.Category != null ? src.Category.Icon : string.Empty))
                .ForMember(dest => dest.WalletName, opt => opt.MapFrom(src => src.Wallet != null ? src.Wallet.Name : string.Empty));
                
            CreateMap<TransactionDto, Transaction>();

            // Budget Maps (Include flattening mapping)
            CreateMap<Budget, BudgetDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.CategoryColor, opt => opt.MapFrom(src => src.Category != null ? src.Category.Color : string.Empty))
                .ForMember(dest => dest.CategoryIcon, opt => opt.MapFrom(src => src.Category != null ? src.Category.Icon : string.Empty));

            CreateMap<BudgetDto, Budget>();

            // FinancialGoal Maps (Include flattening mapping)
            CreateMap<FinancialGoal, FinancialGoalDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.CategoryColor, opt => opt.MapFrom(src => src.Category != null ? src.Category.Color : string.Empty))
                .ForMember(dest => dest.CategoryIcon, opt => opt.MapFrom(src => src.Category != null ? src.Category.Icon : string.Empty));

            CreateMap<FinancialGoalDto, FinancialGoal>();
        }
    }
}
