using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Taxi_Booking_Management.Common;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.Helper;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Models;
using X.PagedList;

namespace Taxi_Booking_Management.Services.TaxiDriver
{
    public class TaxiDriverService : ITaxiDriverService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerManager _loggerManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public TaxiDriverService(ApplicationDbContext dbContext, ILoggerManager loggerManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = dbContext;
            _loggerManager = loggerManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> DeleteTaxiDriverAsync(int driverId)
        {
            string message = MessagesAlerts.FailDelete;
            try
            {
                var exDriver = await _context.drivers.FirstOrDefaultAsync(u => u.DriverId == driverId);
                if (exDriver == null)
                {
                    _loggerManager.LogInfo($"taxi driver not found by given id{driverId}");
                    return message;
                }
                _context.drivers.Remove(exDriver);
                await _context.SaveChangesAsync();
                message = MessagesAlerts.SuccessfullDelete;
                _loggerManager.LogInfo($"taxi driver is successfully Deleted with given id{driverId}");
                return message;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: DeleteTaxiDriverAsync");
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
                _loggerManager.LogInfo($"all taxidriver records are retrived");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetAllTaxiDriverAsync");
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
                    _loggerManager.LogInfo($"taxi driver not found by given id{driverId}");
                    return null;
                }
                _loggerManager.LogInfo($"taxi driver is successfully retrived with given id{driverId}");
                return exDriver;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetTaxiDriverAsync");
                throw;
            }
        }

        public async Task<string> RegisterTaxiDriverAsync(Models.TaxiDriver taxiDriver)
        {
            string message = MessagesAlerts.FailSave;
            try
            {
                var filePath = "/";
                if (taxiDriver.Filename != null && taxiDriver.Filename.Length > 0)
                {
                    filePath = await FileHelper.SaveFileAsync(taxiDriver.Filename, _webHostEnvironment.WebRootPath);
                }
                var exDriver = await _context.drivers.FirstOrDefaultAsync(u => u.DriverMobile == taxiDriver.DriverMobile);
                if (exDriver != null)
                {
                    _loggerManager.LogInfo("taxi driver mobile is already exist");
                    return message;
                }
                taxiDriver.FilePath = filePath;
                await _context.drivers.AddAsync(taxiDriver);
                await _context.SaveChangesAsync();
                message = MessagesAlerts.SuccessfullSave;
                _loggerManager.LogInfo($"taxi driver {MessagesAlerts.SuccessfullSave} with name {taxiDriver.DriverName}");
                return message;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: RegisterTaxiDriverAsync");
                throw;
            }
        }

        public async Task<string?> UpdateTaxiDriverAsync(Models.TaxiDriver taxiDriver)
        {
            string message = MessagesAlerts.FailUpdate;
            try
            {
                var filePath = "/";
                if (taxiDriver.Filename != null && taxiDriver.Filename.Length > 0)
                {
                    filePath = await FileHelper.SaveFileAsync(taxiDriver.Filename, _webHostEnvironment.WebRootPath);
                }
                var exDriver = await _context.drivers.FirstOrDefaultAsync(u => u.DriverId == taxiDriver.DriverId);
                if (exDriver == null)
                {
                    _loggerManager.LogInfo("taxi driver not found");
                    return message;
                }
                exDriver.DriverMobile = taxiDriver.DriverMobile;
                exDriver.DriverName = taxiDriver.DriverName;
                exDriver.Address = taxiDriver.Address;
            
                if (filePath != "/")
                {
                    exDriver.FilePath = filePath;
                }
                else
                {
                    exDriver.FilePath = taxiDriver.FilePath;
                }
                _context.drivers.Update(exDriver);
                await _context.SaveChangesAsync();
                message = MessagesAlerts.SuccessfullUpdate;
                _loggerManager.LogInfo($"taxi driver {MessagesAlerts.SuccessfullUpdate} {taxiDriver.DriverName}");
                return message;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: UpdateTaxiDriverAsync");
                throw;
            }
        }
        public string GenerateHtmlContentForPdf(IPagedList<Models.TaxiDriver> taxiDrivers)
        {
            // Create an HTML table with student data
            var htmlBuilder = new StringBuilder();
            htmlBuilder.Append("<html><head>");
            htmlBuilder.Append("<style>");
            htmlBuilder.Append("table { border-collapse: collapse; width: 100%; border: 1px solid #000; }");
            htmlBuilder.Append("th, td { border: 1px solid #000; padding: 8px; }");
            htmlBuilder.Append("</style>");
            htmlBuilder.Append("</head><body>");
            htmlBuilder.Append("<table>");
            htmlBuilder.Append("<thead><tr><th>Driver Name</th><th>Mobile Number</th><th>Address</th></tr></thead>");
            htmlBuilder.Append("<tbody>");

            foreach (var items in taxiDrivers)
            {
                htmlBuilder.Append("<tr>");
                htmlBuilder.Append($"<td>{items.DriverName}</td>");
                htmlBuilder.Append($"<td>{items.DriverMobile}</td>");
                htmlBuilder.Append($"<td>{items.Address}</td>");
                htmlBuilder.Append("</tr>");
            }

            htmlBuilder.Append("</tbody></table>");

            return htmlBuilder.ToString();
        }

    }
}
