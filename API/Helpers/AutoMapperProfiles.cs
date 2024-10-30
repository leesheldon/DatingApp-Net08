using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDto>()
            .ForMember(d => d.Age, o => o.MapFrom(s => s.DateOfBirth.CalculateAge()))
            .ForMember(d => d.PhotoUrl, 
                o => o.MapFrom(s => s.Photos.FirstOrDefault(x => x.IsMain)!.Url));
        // Purpose of input the "!": 
        // If there is no item in Photos, PhotoUrl will be set to Null, instead of throw an exception.
        
        CreateMap<Photo, PhotoDto>();
        CreateMap<MemberUpdateDto, AppUser>();
    }
}
