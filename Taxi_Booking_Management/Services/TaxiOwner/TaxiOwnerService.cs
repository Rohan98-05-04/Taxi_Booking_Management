
using Microsoft.EntityFrameworkCore;
using Taxi_Booking_Management.Common;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.LoggerService;
using X.PagedList;

namespace Taxi_Booking_Management.Services.TaxiOwner
{
    public class TaxiOwnerService : ITaxiOwnerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerManager _loggerManager;
        public TaxiOwnerService(ApplicationDbContext dbContext , ILoggerManager loggerManager)
        {
            _context = dbContext;
            _loggerManager = loggerManager;
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
              var exOwner = await  _context.owner.FirstOrDefaultAsync(u => u.TaxiOwnerMobile == taxiOwner.TaxiOwnerMobile);
                if (exOwner != null)
                {
                    _loggerManager.LogInfo("taxiowner mobile is already exist");
                    return message;
                }
               await _context.owner.AddAsync(taxiOwner);
               await _context.SaveChangesAsync();
                message = MessagesAlerts.SuccessfullSave;
                _loggerManager.LogInfo($"taxiOwner is successfully register with name {taxiOwner.TaxiOwnerName}");
                return message;
            }catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: RegisterTaxiOwnerAsync");
                throw;
            }
        }
    }
}
