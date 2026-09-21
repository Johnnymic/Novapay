using AutoMapper;
using Nova.Application.Dto.Response;
using Nova.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Mappers
{
    public class WalletProfile : Profile
    {
        public WalletProfile()
        {
            CreateMap<Wallet, WalletResponse>()
                .ForMember(
                    dest => dest.walletId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(
                    dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(
                    dest => dest.Balance,
                    opt => opt.MapFrom(src => src.Balance))
                .ForMember(
                    dest => dest.CustomerId,
                    opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(
                    dest => dest.Currency,
                    opt => opt.MapFrom(src => src.Currency))
                .ForMember(
                    dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => src.CreatedAtUtc));
        }
    }
}
