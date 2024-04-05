using Microsoft.EntityFrameworkCore;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.DtoModels;
using Taxi_Booking_Management.Helper.PdfFormats;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Services.Account;
using static Taxi_Booking_Management.Common.Enums;

namespace Taxi_Booking_Management.Services.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerManager _loggerManager;
        private readonly IRazorViewToStringRenderer _razorViewToStringRenderer;
        public AccountService(ApplicationDbContext dbContext, ILoggerManager loggerManager, IRazorViewToStringRenderer razorViewToStringRenderer)
        {
            _context = dbContext;
            _loggerManager = loggerManager;
            _razorViewToStringRenderer = razorViewToStringRenderer;
        }
        public YearlyTotalModel GetYearlyTotalBookingAmounts(int year)
        {
            try
            {
                decimal totalAmount = _context.Bookings
                    .Where(b => b.CreatedDateTime.Year == year)
                    .Sum(x => x.NetAmount);

                decimal totalPayAmount = _context.PaymentHistories
                    .Where(p => p.CreateDateTime.Year == year)
                    .Sum(p => p.PayAmount);

                decimal paymentsByCash = _context.PaymentHistories
                    .Where(p => p.CreateDateTime.Year == year && p.PaidMedium == (int)PaymentMedium.Cash)
                    .Sum(p => p.PayAmount);

                decimal paymentsOnline = _context.PaymentHistories
                    .Where(p => p.CreateDateTime.Year == year && p.PaidMedium == (int)PaymentMedium.Online)
                    .Sum(p => p.PayAmount);

                _loggerManager.LogInfo($"All account summary of {year} Year retrived Successfully");

                return new YearlyTotalModel
                {
                    Year = year,
                    TotalAmount = totalAmount,
                    TotalPayAmount = totalPayAmount,
                    PaymentsByCash = paymentsByCash,
                    PaymentsOnline = paymentsOnline,
                    DueAmount=totalAmount-totalPayAmount
                };
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message}, method name: GetYearlyTotalBookingAmounts");
                throw;
            }
        }

        public DateTotalAmountModel GetDateBillAmounts(DateTime fromdate, DateTime todate)
        {
            try
            {
                decimal totalAmount = _context.Bookings
                    .Where(x => x.CreatedDateTime >= fromdate && x.CreatedDateTime <= todate.AddDays(1))
                    .Sum(x => x.NetAmount);

                decimal totalPayAmount = _context.PaymentHistories
                    .Where(x => x.CreateDateTime >= fromdate && x.CreateDateTime <= todate.AddDays(1))
                    .Sum(x => x.PayAmount);

                decimal paymentsByCash = _context.PaymentHistories
                    .Where(x => x.CreateDateTime >= fromdate && x.CreateDateTime <= todate.AddDays(1) && x.PaidMedium == (int)PaymentMedium.Cash)
                    .Sum(x => x.PayAmount);

                decimal paymentsOnline = _context.PaymentHistories
                    .Where(x => x.CreateDateTime >= fromdate && x.CreateDateTime <= todate.AddDays(1) && x.PaidMedium == (int)PaymentMedium.Online)
                    .Sum(x => x.PayAmount);

                _loggerManager.LogInfo($"All account summary from {fromdate} to {todate} retrived Successfully");

                return new DateTotalAmountModel
                {
                   fromDate = fromdate,
                   toDate = todate,
                    TotalAmount = totalAmount,
                    TotalPayAmount = totalPayAmount,
                    PaymentsByCash = paymentsByCash,
                    PaymentsOnline = paymentsOnline,
                    DueAmount = totalAmount - totalPayAmount
                };
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message}, method name: GetDateBillAmounts");
                throw;
            }
        }
       

        public async Task<string> CreatePdfForAnnualAmount(DtoModels.YearlyTotalModel yearTotalAmountData)
        {
         
            var htmlContent = await _razorViewToStringRenderer.RenderViewToStringAsync("AnnualPaymentPdf", yearTotalAmountData);

            return htmlContent;
        }

        public async Task<string> CreatePdfForDateAmount(DtoModels.DateTotalAmountModel TotalAmountForDate)
        {

            var htmlContent = await _razorViewToStringRenderer.RenderViewToStringAsync("TotalAmountforDatesPdf", TotalAmountForDate);

            return htmlContent;
        }
    }
}