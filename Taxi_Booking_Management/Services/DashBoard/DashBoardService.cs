using Microsoft.EntityFrameworkCore;
using System.Linq;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.LoggerService;

namespace Taxi_Booking_Management.Services.DashBoard
{
    public class DashBoardService : IDashBoardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerManager _loggerManager;
        public DashBoardService(ApplicationDbContext dbContext , ILoggerManager loggerManager)
        {
            _context = dbContext;
            _loggerManager = loggerManager;
        }

        public Dictionary<string, decimal> GetDailyTotalBillAmounts(DateTime date)
        {
            Dictionary<string, decimal> hourlyTotalBookingAmounts = new Dictionary<string, decimal>();
            try
            {
                for(int hour =0; hour < 24; hour += 3)
                {
                    DateTime startHour = date.AddHours(hour);
                    DateTime endHour = startHour.AddHours(3);

                    decimal totalBookingAmount = _context.Bookings
                                               .Where(x => x.CreatedDateTime >= startHour
                                                 && x.CreatedDateTime < endHour).Sum(x => x.NetAmount);

                    hourlyTotalBookingAmounts.Add($"{startHour:hh:mm tt} - {endHour:hh:mm tt}", totalBookingAmount);
                    _loggerManager.LogInfo($"all Bookings recors revired daily wise wise in year {date}");
                }
            }catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetDailyTotalBillAmounts");
                throw;
            }
            return hourlyTotalBookingAmounts;
        }

        public List<decimal> GetMonthlyTotalBookingAmounts(int year)
        {
            
            List<decimal> monthlyTotalBookingAmount = new List<decimal>();
            try
            {
                for (int month = 1; month <= 12; month++)
                {
                    decimal totalAmount = _context.Bookings.Where(x => x.FromDate.Year == year && x.FromDate.Month == month)
                                                           .Sum(x => x.NetAmount);
                    monthlyTotalBookingAmount.Add(totalAmount);
                }
                _loggerManager.LogInfo($"all Bookings recors revired monthly wise in year {year}");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetMonthlyTotalBookingAmounts");
                throw;
            }
            return monthlyTotalBookingAmount;
        }

        public async Task<int> GetTotalBooking()
        {
            int BookingCount = 0;
            try
            {
                BookingCount = await _context.Bookings.CountAsync();
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetTotalCustomer");
                throw;
            }
            return BookingCount;
        }

        public async Task<int> GetTotalCustomer()
        {
            int CustomerCount = 0;
            try
            {
             CustomerCount = await _context.Bookings.Select(x => x.CustomerMobile).Distinct().CountAsync();
            }catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetTotalCustomer");
                throw;
            }
            return CustomerCount;
        }

        public async Task<int> GetTotalDrivers()
        {
            int DriverCount ;
            try
            {
                DriverCount = await _context.drivers.CountAsync();
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetTotalDrivers");
                throw;
            }
            return DriverCount;
        }

        public async Task<int> GetTotalTaxiCount()
        {
            int TaxiCount;
            try
            {
                TaxiCount = await _context.taxis.CountAsync();
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetTotalTaxiCount");
                throw;
            }
            return TaxiCount;
        }

        public async Task<int> GetTotalTaxiOwner()
        {
            int TaxiOwners;
            try
            {
                TaxiOwners = await _context.owner.CountAsync();
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetTotalTaxiCount");
                throw;
            }
            return TaxiOwners;
        }
    }
}
