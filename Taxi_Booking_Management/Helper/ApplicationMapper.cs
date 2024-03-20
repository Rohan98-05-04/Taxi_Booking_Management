using AutoMapper;
using Taxi_Booking_Management.DtoModels;
using Taxi_Booking_Management.Models;

namespace Taxi_Booking_Management.Helper
{
    public class ApplicationMapper :Profile
    {
        public ApplicationMapper()
        {
            CreateMap<Taxi, TaxiDto>()
           .ForMember(dest => dest.TaxiOwnerName, opt => opt.MapFrom(src => src.TaxiOwner.TaxiOwnerName))
           .ForMember(dest => dest.TaxiOwnerMobile, opt => opt.MapFrom(src => src.TaxiOwner.TaxiOwnerMobile));

            CreateMap<User, UpdateUserDto>().ReverseMap();
            
            CreateMap<Booking, BookingDto>()
           .ForMember(dest => dest.TaxiName, opt => opt.MapFrom(src => src.taxi.TaxiName))
           .ForMember(dest => dest.RegistrationNumber, opt => opt.MapFrom(src => src.taxi.RegistrationNumber));

            CreateMap<RegisterBookingDto, Booking>().ReverseMap();
            CreateMap<TaxiViewModel, Taxi>();
            CreateMap<EditBookingViewModel, Booking>();

            CreateMap<Booking, CustomerViewModel>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.CustomerName))
                .ForMember(dest => dest.CustomerMobile, opt => opt.MapFrom(src => src.CustomerMobile));


        }
    }
}
