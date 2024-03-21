using Microsoft.EntityFrameworkCore;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.LoggerService;

namespace Taxi_Booking_Management.Services.Accounts
{
    public class AccountService :  IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerManager _loggerManager;
        public AccountService(ApplicationDbContext dbContext, ILoggerManager loggerManager)
        {
            _context = dbContext;
            _loggerManager = loggerManager;
        }
        public decimal GetYearlyTotalBookingAmounts(int year)
        {
            decimal sum = 0;
            List<decimal> yearlyTotalBookingAmounts = new List<decimal>();
            try
            {
                for (int month = 1; month <= 12; month++)
                {
                    decimal totalAmount = _context.Bookings.Where(x => x.FromDate.Year == year && x.FromDate.Month == month)
                                                           .Sum(x => x.NetAmount);
                    yearlyTotalBookingAmounts.Add(totalAmount);
                    sum = yearlyTotalBookingAmounts.Sum();
                }
                _loggerManager.LogInfo($"all Bookings recors revired in year {year}");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetYearlyTotalBookingAmounts");
                throw;
            }
            return sum;
        }

        public decimal GetDateBillAmounts(DateTime fromdate, DateTime todate)
        {
            decimal sum = 0;
            try
            {
                sum = _context.Bookings.Where(x => x.CreatedDateTime >= fromdate
                                                 && x.CreatedDateTime < todate).Sum(x => x.NetAmount);
                _loggerManager.LogInfo($"all Bookings recors revired date wise wise in from {fromdate} to {todate}");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetDailyTotalBillAmounts");
                throw;
            }
            return sum;
        }
    }
}
