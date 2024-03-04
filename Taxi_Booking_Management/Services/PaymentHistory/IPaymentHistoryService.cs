using Taxi_Booking_Management.Models;


namespace Taxi_Booking_Management.Services.PaymentHistory
{
    public interface IPaymentHistoryService
    {
        Task<int> CreatePayment(Models.PaymentHistory paymentHistory);
        Task<List<Models.PaymentHistory>> GetAllPayments();
        Task<Models.PaymentHistory> GetIndividualPaymentByBookingId(int bookingId);
    }
}
