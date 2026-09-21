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
    public class WalletMappingProfile : Profile
    {
        public WalletMappingProfile()
        {
            CreateMap<WalletTransaction, CreditWalletResponse>()
                .ForMember(
                    dest => dest.TransactionId,
                    opt => opt.MapFrom(src => src.Id))

                .ForMember(
                    dest => dest.WalletId,
                    opt => opt.MapFrom(src => src.WalletId))

                .ForMember(
                    dest => dest.Amount,
                    opt => opt.MapFrom(src => src.Amount))

                .ForMember(
                    dest => dest.BalanceBefore,
                    opt => opt.MapFrom(src => src.AmountBalanceBeforeTransaction))

                .ForMember(
                    dest => dest.BalanceAfter,
                    opt => opt.MapFrom(src => src.AmountBalanceAfterTransaction))

                .ForMember(
                    dest => dest.Currency,
                    opt => opt.MapFrom(src => src.Wallet.Currency))

                .ForMember(
                    dest => dest.Reference,
                    opt => opt.MapFrom(src => src.Reference))

                .ForMember(
                    dest => dest.CreatedAtUtc,
                    opt => opt.MapFrom(src => src.CreatedAtUtc));
        }
    }
}
