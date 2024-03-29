
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using Taxi_Booking_Management.Common;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.Helper;
using Taxi_Booking_Management.Helper.PdfFormats;
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
        private readonly IRazorViewToStringRenderer _razorViewToStringRenderer;

        public TaxiOwnerService(IMemoryCache memoryCache ,ApplicationDbContext dbContext , 
            ILoggerManager loggerManager, IWebHostEnvironment webHostEnvironment, IRazorViewToStringRenderer razorViewToStringRenderer)
        {
            _context = dbContext;
            _loggerManager = loggerManager;
            _memoryCache = memoryCache;
            _webHostEnvironment = webHostEnvironment;
            _razorViewToStringRenderer = razorViewToStringRenderer;
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
                exOwner.IsEnabled = false;
                _context.owner.Update(exOwner);
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
                IQueryable<Models.TaxiOwner> data = _context.owner.Where(u => u.IsEnabled).AsQueryable();
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
              var exOwner = await _context.owner.FirstOrDefaultAsync(u => u.TaxiOwnerId == ownerId && u.IsEnabled);
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
                taxiOwner.IsEnabled = true;
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
                exOwner.IsEnabled = true;
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
                taxiOwner = _context.owner.Where(u=> u.IsEnabled)
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

       

        public async Task<string> GenerateHtmlContentForPdf(IEnumerable<Models.TaxiOwner> taxiOwnerData)
        {
            var htmlContent = await _razorViewToStringRenderer.RenderViewToStringAsync("TaxiOwnerPdf", taxiOwnerData);

            return htmlContent;
        }
    }
}
