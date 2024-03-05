using Taxi_Booking_Management.Models;
using X.PagedList;


namespace Taxi_Booking_Management.Services.PaymentHistory
{
    public interface IPaymentHistoryService
    {
        Task<int> CreatePayment(Models.PaymentHistory paymentHistory);
        Task<IPagedList<Models.PaymentHistory>> GetAllPayments( int pageNumber, int pageSize, string? startDate, string? endDate);
        Task<Models.PaymentHistory> GetIndividualPaymentByBookingId(int bookingId);
    }
}
