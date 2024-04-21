using AutoMapper;
using JoliDay.Dto;
using JoliDay.Models;
using JoliDay.ViewModel;

namespace JoliDay.Config
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            #region Model -> Dto

            CreateMap<User, UserDto>();
            CreateMap<Holiday, HolidayDto>();
            CreateMap<Address, AddressDto>();
            CreateMap<Activity, ActivityDto>();
            CreateMap<Message, MessageDto>();
            CreateMap<Invite, InviteDto>()
                .ForMember(e => e.Title, opt => opt.MapFrom(e => e.Holiday.Name));

            #endregion

            #region ViewModel -> Model

            CreateMap<EditHolidayViewModel, Holiday>();
            CreateMap<CreateHolidayViewModel, Holiday>();
            CreateMap<AddressViewModel, Address>();
            CreateMap<ActivityViewModel, Activity>();

            #endregion
        }
    }
}