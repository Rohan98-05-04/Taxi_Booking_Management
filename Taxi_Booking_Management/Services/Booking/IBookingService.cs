using Taxi_Booking_Management.DtoModels;
using X.PagedList;

namespace Taxi_Booking_Management.Services.Booking
{
    public interface IBookingService
    {
        public Task<string> RegisterBookingAsync(RegisterBookingDto bookingDto);
        public Task<IPagedList<Models.Booking>> GetAllBookingDetailsAsync(int page, int pageSize, string search);
        public Task<BookingDto?> GetTaxiBookingAsync(int bookingId);
        public Task<bool> IsTaxiAvailableAsync(int taxiId, DateTime fromDate, DateTime toDate);
        public Task<int> GetTaxiIdByRegNo(string regNo);
        public Task<IList<Models.Booking>> GetTaxiAvailableDates(int taxiId);
    }
}
