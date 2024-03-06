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
           .ForMember(dest => dest.DriverName, opt => opt.MapFrom(src => src.Driver.DriverName))
           .ForMember(dest => dest.DriverMobile, opt => opt.MapFrom(src => src.Driver.DriverMobile))
           .ForMember(dest => dest.TaxiOwnerName, opt => opt.MapFrom(src => src.TaxiOwner.TaxiOwnerName))
           .ForMember(dest => dest.TaxiOwnerMobile, opt => opt.MapFrom(src => src.TaxiOwner.TaxiOwnerMobile));



            CreateMap<Booking, BookingDto>()
           .ForMember(dest => dest.TaxiName, opt => opt.MapFrom(src => src.taxi.TaxiName))
           .ForMember(dest => dest.RegistrationNumber, opt => opt.MapFrom(src => src.taxi.RegistrationNumber));


            CreateMap<RegisterBookingDto, Booking>().ReverseMap();
            CreateMap<TaxiViewModel, Taxi>();

        }
    }
}
