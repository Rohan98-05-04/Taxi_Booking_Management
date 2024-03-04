using Microsoft.EntityFrameworkCore;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.LoggerService;

namespace Taxi_Booking_Management.Services.PaymentHistory
{
    public class PaymentHistoryService : IPaymentHistoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerManager _loggerManager;
        public PaymentHistoryService(ApplicationDbContext context, ILoggerManager loggerManager)
        {
            _context = context;
            _loggerManager = loggerManager;
        }
        public async Task<int> CreatePayment(Models.PaymentHistory paymentHistory)
        {
            try
            {
                paymentHistory.createDateTime = DateTime.Now;
                paymentHistory.updateDateTime = DateTime.Now;

                _context.PaymentHistories.Add(paymentHistory);
                await _context.SaveChangesAsync();

                _loggerManager.LogInfo( "Payment created successfully.");

                return paymentHistory.PaymentId;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError( "Error occurred while creating payment.");
                throw new ApplicationException("Error occurred while creating payment.", ex);
            }
        }

        public async Task<List<Models.PaymentHistory>> GetAllPayments()
        {
            try
            {
                var payments = await _context.PaymentHistories.ToListAsync();

                _loggerManager.LogInfo( "Retrieved all payments successfully.");

                return payments;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError( "Error occurred while retrieving all payments.");
                throw new ApplicationException("Error occurred while retrieving all payments.", ex);
            }
        }

        public async Task<Models.PaymentHistory> GetIndividualPaymentByBookingId(int bookingId)
        {
            try
            {
                var payment = await _context.PaymentHistories
                    .FirstOrDefaultAsync(ph => ph.BookingId == bookingId);

                _loggerManager.LogInfo( $"Retrieved payment for BookingId: {bookingId} successfully.");

                return payment;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError( $"Error occurred while retrieving payment for BookingId: {bookingId}.");
                throw new ApplicationException($"Error occurred while retrieving payment for BookingId: {bookingId}.", ex);
            }
        }

    }
}
