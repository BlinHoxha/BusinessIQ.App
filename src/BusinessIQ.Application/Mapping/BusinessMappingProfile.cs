using AutoMapper;
using BuildingBlocks.AI.Core.Models;
using Framework.Contracts.AI;
using BusinessIQ.Contracts.Businesses;
using BusinessIQ.Domain.Businesses;

namespace BusinessIQ.Application.Mapping;

public sealed class BusinessMappingProfile : Profile
{
    public BusinessMappingProfile()
    {
        CreateMap<DocumentProfile, DocumentProfileResponse>();
        CreateMap<Business, BusinessDto>();
        CreateMap<Business, BusinessGridDto>();
        CreateMap<CreateBusinessDto, Business>()
            .ForMember(x => x.Id, o => o.Ignore())
            .ForMember(x => x.OrganizationId, o => o.Ignore())
            .ForMember(x => x.IsArchived, o => o.Ignore())
            .ForMember(x => x.Name, o => o.MapFrom(s => s.Name.Trim()))
            .ForMember(x => x.Industry, o => o.MapFrom(s => s.Industry.Trim()));
        CreateMap<UpdateBusinessDto, Business>()
            .ForMember(x => x.Id, o => o.Ignore())
            .ForMember(x => x.OrganizationId, o => o.Ignore())
            .ForMember(x => x.IsArchived, o => o.Ignore())
            .ForMember(x => x.Name, o => o.MapFrom(s => s.Name.Trim()))
            .ForMember(x => x.Industry, o => o.MapFrom(s => s.Industry.Trim()));
    }
}
