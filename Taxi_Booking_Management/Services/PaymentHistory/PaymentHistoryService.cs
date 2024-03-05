using Microsoft.EntityFrameworkCore;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.LoggerService;
using X.PagedList;

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

        public async Task<IPagedList<Models.PaymentHistory>> GetAllPayments( int pageNumber, int pageSize)
        {
            try
            {
                var payments = await _context.PaymentHistories.ToListAsync();

                _loggerManager.LogInfo( "Retrieved all payments successfully.");

                var pagedPayments = payments.ToPagedList(pageNumber, pageSize);

                return pagedPayments;
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

                if (payment != null)
                {
                    if (!Enum.IsDefined(typeof(Taxi_Booking_Management.Common.Enums.PaymentMedium), payment.PaidMedium))
                    {
                        _loggerManager.LogInfo($"Invalid payment medium: {payment.PaidMedium}");
                        throw new InvalidOperationException($"Invalid payment medium {payment.PaidMedium}");
                    }

                    _loggerManager.LogInfo($"Retrieved payment for BookingId: {bookingId} successfully.");
                    return payment;
                }
                else
                {
                    _loggerManager.LogError($"Payment not found for BookingId: {bookingId}.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"Error occurred while retrieving payment for BookingId: {bookingId}.", ex);
                throw new ApplicationException($"Error occurred while retrieving payment for BookingId: {bookingId}.", ex);
            }
        }


    }
}
