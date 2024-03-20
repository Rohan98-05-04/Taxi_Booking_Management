
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using Taxi_Booking_Management.Common;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.Helper;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Models;
using X.PagedList;

namespace Taxi_Booking_Management.Services.TaxiOwner
{
    public class TaxiOwnerService : ITaxiOwnerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerManager _loggerManager;
        private readonly IMemoryCache _memoryCache;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TaxiOwnerService(IMemoryCache memoryCache ,ApplicationDbContext dbContext , ILoggerManager loggerManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = dbContext;
            _loggerManager = loggerManager;
            _memoryCache = memoryCache;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> DeleteTaxiOwnerAsync(int ownerId)
        {
            string message = MessagesAlerts.FailDelete;
            try
            {
                var exOwner = await _context.owner.FirstOrDefaultAsync(u => u.TaxiOwnerId == ownerId);
                if (exOwner == null)
                {
                    _loggerManager.LogInfo($"taxi owner not found by given id{ownerId}");
                    return message;
                }
                _context.owner.Remove(exOwner);
                await _context.SaveChangesAsync();
                message = MessagesAlerts.SuccessfullDelete;
                _loggerManager.LogInfo($"taxi owner is successfully retrived with given id{ownerId}");
                return message;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: DeleteTaxiOwnerAsync");
                throw;
            }
        }
        public async Task<IPagedList<Models.TaxiOwner>> GetAllTaxiOwnerAsync(int page, int pageSize, string search)
        {
            IPagedList<Models.TaxiOwner> taxiOwners = null;
            try
            {
                IQueryable<Models.TaxiOwner> data = _context.owner.AsQueryable();
                if (!string.IsNullOrWhiteSpace(search) && data != null)
                {
                    data = data.Where(u => u.TaxiOwnerName.Contains(search));

                }
                taxiOwners =await data.ToPagedListAsync(page, pageSize);
                _loggerManager.LogInfo($"all taxiowner records are retrived");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetAllTaxiOwnerAsync");
                throw;
            }
            return taxiOwners;
        }

        public async Task<Models.TaxiOwner?> GetTaxiOwnerAsync(int ownerId)
        {
            try
            {
              var exOwner = await _context.owner.FirstOrDefaultAsync(u => u.TaxiOwnerId == ownerId);
                if(exOwner == null)
                {
                    _loggerManager.LogInfo($"taxiowner not found by given id{ownerId}");
                    return null;
                }
                _loggerManager.LogInfo($"taxiowner is successfully retrived with given id{ownerId}");
                return exOwner;
            }catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetTaxiOwnerAsync");
                throw;
            }
        }

        public async Task<string> RegisterTaxiOwnerAsync(Models.TaxiOwner taxiOwner)
        {
            string message = MessagesAlerts.FailSave;
            try
            {
                var filePath = "/";
                if (taxiOwner.Filename != null && taxiOwner.Filename.Length > 0)
                {
                    filePath = await FileHelper.SaveFileAsync(taxiOwner.Filename, _webHostEnvironment.WebRootPath);
                }
                var exOwner = await  _context.owner.FirstOrDefaultAsync(u => u.TaxiOwnerMobile == taxiOwner.TaxiOwnerMobile);
                if (exOwner != null)
                {
                    _loggerManager.LogInfo("taxiowner mobile is already exist");
                    return message;
                }
                taxiOwner.FilePath = filePath;
                await _context.owner.AddAsync(taxiOwner);
               await _context.SaveChangesAsync();
                message = MessagesAlerts.SuccessfullSave;
                _loggerManager.LogInfo($"taxiOwner {MessagesAlerts.SuccessfullSave} with name {taxiOwner.TaxiOwnerName}");
                return message;
            }catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: RegisterTaxiOwnerAsync");
                throw;
            }
        }

        public async Task<string?> UpdateTaxiOwner(Models.TaxiOwner taxiOwner)
        {
            string message = MessagesAlerts.FailUpdate;
            try
            {
                var filePath = "/";
                if (taxiOwner.Filename != null && taxiOwner.Filename.Length > 0)
                {
                    filePath = await FileHelper.SaveFileAsync(taxiOwner.Filename, _webHostEnvironment.WebRootPath);
                }
                var exOwner = await _context.owner.FirstOrDefaultAsync(u => u.TaxiOwnerMobile == taxiOwner.TaxiOwnerMobile);
                if (exOwner == null)
                {
                    _loggerManager.LogInfo($"taxiowner not found with {taxiOwner.TaxiOwnerId}");
                    return message;
                }
                exOwner.TaxiOwnerMobile = taxiOwner.TaxiOwnerMobile;
                exOwner.TaxiOwnerName = taxiOwner.TaxiOwnerName;
                exOwner.TaxiOwnerEmail = taxiOwner.TaxiOwnerEmail;
                exOwner.TaxiOwnerAddress = taxiOwner.TaxiOwnerAddress;
                if (filePath != "/")
                {
                    exOwner.FilePath = filePath;
                }
                else
                {
                    exOwner.FilePath = taxiOwner.FilePath;
                }
              
                _context.owner.Update(exOwner);
                await _context.SaveChangesAsync();
                message = MessagesAlerts.SuccessfullSave;
                _loggerManager.LogInfo($"taxiOwner {MessagesAlerts.SuccessfullUpdate} with name {taxiOwner.TaxiOwnerName}");
                return message;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: UpdateTaxiOwner");
                throw;
            }
        }

        public IEnumerable<SelectListItem> GetTaxiOwners()
        {
            const string cacheKey = "TaxiOwnerCacheKey";
            if(!_memoryCache.TryGetValue(cacheKey, out IEnumerable<SelectListItem> taxiOwner))
            {
                taxiOwner = _context.owner
                .Select(x => new SelectListItem { Value = x.TaxiOwnerId.ToString(), Text = $"{x.TaxiOwnerName} ({x.TaxiOwnerMobile})" })
                .ToList();

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                };

                _memoryCache.Set(cacheKey, taxiOwner, cacheEntryOptions);
            }
            return taxiOwner;
        }

        public string GenerateHtmlContentForPdf(IPagedList<Models.TaxiOwner> taxiOwners)
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
            htmlBuilder.Append("<thead><tr><th>Owner Name</th><th>Mobile Number</th><th>Email</th><th>Address</th></tr></thead>");
            htmlBuilder.Append("<tbody>");

            foreach (var owners in taxiOwners)
            {
                htmlBuilder.Append("<tr>");
                htmlBuilder.Append($"<td>{owners.TaxiOwnerName}</td>");
                htmlBuilder.Append($"<td>{owners.TaxiOwnerMobile}</td>");
                htmlBuilder.Append($"<td>{owners.TaxiOwnerEmail}</td>");
                htmlBuilder.Append($"<td>{owners.TaxiOwnerAddress}</td>");
                htmlBuilder.Append("</tr>");
            }

            htmlBuilder.Append("</tbody></table>");

            return htmlBuilder.ToString();
        }
    }
}
