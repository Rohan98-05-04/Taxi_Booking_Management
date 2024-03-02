using Microsoft.EntityFrameworkCore;
using Taxi_Booking_Management.Common;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Models;
using X.PagedList;

namespace Taxi_Booking_Management.Services.TaxiDriver
{
    public class TaxiDriverService : ITaxiDriverService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerManger _loggerManger;
        public TaxiDriverService(ApplicationDbContext dbContext, ILoggerManger loggerManger)
        {
            _context = dbContext;
            _loggerManger = loggerManger;
        }

        public async Task<string> DeleteTaxiDriverAsync(int driverId)
        {
            string message = MessagesAlerts.FailDelete;
            try
            {
                var exDriver = await _context.drivers.FirstOrDefaultAsync(u => u.DriverId == driverId);
                if (exDriver == null)
                {
                    _loggerManger.LogInfo($"taxi driver not found by given id{driverId}");
                    return message;
                }
                _context.drivers.Remove(exDriver);
                await _context.SaveChangesAsync();
                message = MessagesAlerts.SuccessfullDelete;
                _loggerManger.LogInfo($"taxi driver is successfully retrived with given id{driverId}");
                return message;
            }
            catch (Exception ex)
            {
                _loggerManger.LogError($"{ex.Message} ,method name: GetTaxiDriverAsync");
                throw;
            }
        }

        public async Task<IPagedList<Models.TaxiDriver>> GetAllTaxiDriverAsync(int page, int pageSize, string search)
        {
            IPagedList<Models.TaxiDriver> taxiDrivers = null;
            try
            {
                IQueryable<Models.TaxiDriver> data = _context.drivers.AsQueryable();
                if (!string.IsNullOrWhiteSpace(search) && data != null)
                {
                    data = data.Where(u => u.DriverName.Contains(search));

                }
                taxiDrivers = await data.ToPagedListAsync(page, pageSize);
                _loggerManger.LogInfo($"all taxidriver records are retrived");
            }
            catch (Exception ex)
            {
                _loggerManger.LogError($"{ex.Message} ,method name: GetAllTaxiDriverAsync");
                throw;
            }
            return taxiDrivers;
        }

        public async Task<Models.TaxiDriver?> GetTaxiDriverAsync(int driverId)
        {
            try
            {
                var exDriver = await _context.drivers.FirstOrDefaultAsync(u => u.DriverId == driverId);
                if (exDriver == null)
                {
                    _loggerManger.LogInfo($"taxi driver not found by given id{driverId}");
                    return null;
                }
                _loggerManger.LogInfo($"taxi driver is successfully retrived with given id{driverId}");
                return exDriver;
            }
            catch (Exception ex)
            {
                _loggerManger.LogError($"{ex.Message} ,method name: GetTaxiDriverAsync");
                throw;
            }
        }

        public async Task<string> RegisterTaxiDriverAsync(Models.TaxiDriver taxiDriver)
        {
            string message = MessagesAlerts.FailSave;
            try
            {
                var exDriver = await _context.drivers.FirstOrDefaultAsync(u => u.DriverMobile == taxiDriver.DriverMobile);
                if (exDriver != null)
                {
                    _loggerManger.LogInfo("taxi driver mobile is already exist");
                    return message;
                }
                await _context.drivers.AddAsync(taxiDriver);
                await _context.SaveChangesAsync();
                message = MessagesAlerts.SuccessfullSave;
                _loggerManger.LogInfo($"taxi driver is successfully register with name {taxiDriver.DriverName}");
                return message;
            }
            catch (Exception ex)
            {
                _loggerManger.LogError($"{ex.Message} ,method name: RegisterTaxiDriverAsync");
                throw;
            }
        }
    }
}
