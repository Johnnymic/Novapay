using AutoMapper;
using Nova.Application.Dto.Request;
using Nova.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Nova.Application.Mappers
{
    public class AuthMappingProfile : Profile
    {
        public AuthMappingProfile()
        {
            CreateMap<RegisterRequest, Customer>()
                .ForMember(
                    dest => dest.PasswordHash,
                    opt => opt.Ignore())
                .ForMember(
                    dest => dest.Id,
                    opt => opt.Ignore())
                .ForMember(
                    dest => dest.CustomerReference,
                    opt => opt.Ignore())
                .ForMember(
                    dest => dest.KycTier,
                    opt => opt.Ignore())
                .ForMember(
                    dest => dest.Status,
                    opt => opt.Ignore())
                .ForMember(
                    dest => dest.CreatedAt,
                    opt => opt.Ignore())
                .ForMember(
                    dest => dest.Wallets,
                    opt => opt.Ignore());

            
        }
    }
}
