using Taxi_Booking_Management.DtoModels;
using X.PagedList;

namespace Taxi_Booking_Management.Services.Booking
{
    public interface IBookingService
    {
        public Task<IPagedList<Models.Booking>> GetAllBookingDetailsAsync(int page, int pageSize, string search);

        public  Task<BookingDto?> GetTaxiBookingAsync(int bookingId);
    }
}
