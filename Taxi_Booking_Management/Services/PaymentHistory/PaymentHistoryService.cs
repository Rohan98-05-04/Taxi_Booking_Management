using Microsoft.EntityFrameworkCore;
using System.Globalization;
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
        public async Task<int?> CreatePayment(Models.PaymentHistory paymentHistory)
        {
            try
            {
                paymentHistory.createDateTime = DateTime.Now;
                paymentHistory.updateDateTime = DateTime.Now;

                if (!Enum.IsDefined(typeof(Taxi_Booking_Management.Common.Enums.PaymentMedium), paymentHistory.PaidMedium))
                {
                    _loggerManager.LogInfo($"Invalid payment medium: {paymentHistory.PaidMedium}");
                    return null;
                }

                _context.PaymentHistories.Add(paymentHistory);
                await _context.SaveChangesAsync();

                _loggerManager.LogInfo("Payment created successfully.");

                return paymentHistory.PaymentId;
            }
            catch (InvalidOperationException ex)
            {
                _loggerManager.LogError($"Invalid payment medium: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"Error occurred while creating payment: {ex.Message}");
                throw new ApplicationException("Error occurred while creating payment.", ex);
            }
        }


        public async Task<IPagedList<Models.PaymentHistory>> GetAllPayments(int pageNumber, int pageSize, string? startDate, string? endDate)
        {
            try
            {
                var paymentsQuery = _context.PaymentHistories
                    .Include(ph => ph.booking) 
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(startDate) && DateTime.TryParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDateValue))
                {
                    paymentsQuery = paymentsQuery
                        .Where(ph => ph.createDateTime >= startDateValue);
                }

                if (!string.IsNullOrWhiteSpace(endDate) && DateTime.TryParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDateValue))
                { 
                    endDateValue = endDateValue.AddDays(1).Date;
                    paymentsQuery = paymentsQuery
                        .Where(ph => ph.createDateTime < endDateValue);
                }

                var totalCount = await paymentsQuery.CountAsync();
                var paymentsList = await paymentsQuery
                    .OrderByDescending(ph => ph.createDateTime)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var pagedPayments = paymentsList.ToPagedList(pageNumber, pageSize);

                return pagedPayments;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError("Error occurred while retrieving payments.", ex);
                throw new ApplicationException("Error occurred while retrieving payments.", ex);
            }
        }


        public async Task<Models.PaymentHistory?> GetIndividualPaymentById(int paymentId)
        {
            try
            {
                var payment = await _context.PaymentHistories
                    .FirstOrDefaultAsync(ph => ph.PaymentId == paymentId);

                if(payment == null)
                {
                    _loggerManager.LogInfo($"PaymentId not");

                    return null;
                    
                }
                _loggerManager.LogInfo($"Retrieved payment for PaymentId: {paymentId} successfully.");

                return payment;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"Error occurred while retrieving payment for PaymentId: {paymentId}.");
                throw new ApplicationException($"Error occurred while retrieving payment for PaymentId: {paymentId}.", ex);
            }
        }


    }
}
