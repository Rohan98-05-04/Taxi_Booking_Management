
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Taxi_Booking_Management.Common;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.DtoModels;
using Taxi_Booking_Management.LoggerService;
using X.PagedList;

namespace Taxi_Booking_Management.Services.Taxi
{
    public class TaxiService : ITaxiService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerManager _loggerManager;
        private readonly IMapper _mapper;

        public TaxiService(ApplicationDbContext dbContext, ILoggerManager loggerManager , IMapper mapper)
        {
            _context = dbContext;
            _loggerManager = loggerManager;
            _mapper = mapper;
        }

        public async Task<Models.Taxi?> DeleteTaxiAsync(int taxiId)
        {
            try
            {
                var taxi = await _context.taxis.Include(t => t.TaxiOwner).Include(t => t.Driver)
                                                .FirstOrDefaultAsync(t => t.TaxiId == taxiId);
                if (taxi == null)
                {
                    _loggerManager.LogInfo($"not taxi found with taxiId {taxiId}");
                    return null;
                }
                _context.taxis.Remove(taxi);
                await _context.SaveChangesAsync();
                _loggerManager.LogInfo($"taxi {MessagesAlerts.SuccessfullDelete} with taxiId {taxiId}");
                return taxi;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} method name : GetTaxiDetailsAsync");
                throw;
            }
        }

        public async Task<IPagedList<Models.Taxi>> GetAllTaxiDetailsAsync(int page, int pageSize, string search)
        {
            IPagedList<Models.Taxi> taxies = null;
            try
            {
                IQueryable<Models.Taxi> data = _context.taxis
                .Include(t => t.Driver)
                .Include(t => t.TaxiOwner)
                .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search) && data != null)
                {
                    data = data.Where(u => u.TaxiName.Contains(search) || u.RegistrationNumber.Contains(search));

                }
                taxies = await data.ToPagedListAsync(page, pageSize);
                _loggerManager.LogInfo($"all taxi records are retrived");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetAllTaxiDetailsAsync");
                throw;
            }
            return taxies;

        }

        public async Task<TaxiDto?> GetTaxiDetailsAsync(int taxiId)
        {
            try
            {
                var taxi = await  _context.taxis.Include(t => t.TaxiOwner).Include(t => t.Driver)
                                                .FirstOrDefaultAsync(t => t.TaxiId == taxiId);
                if(taxi == null)
                {
                    _loggerManager.LogInfo($"not taxi found with taxiId {taxiId}");
                    return null;
                }
               var taxiDto = _mapper.Map<TaxiDto>(taxi);
                return taxiDto;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} method name : GetTaxiDetailsAsync");
                throw;
            }
        }

        public async Task<string> RegisterTaxiAsync(Models.Taxi taxi)
        {
            try
            {
               var exTaxi = await _context.taxis.FirstOrDefaultAsync(u => u.RegistrationNumber == taxi.RegistrationNumber);
                if(exTaxi != null)
                {
                    _loggerManager.LogInfo($"taxi with register number already exit: {taxi.RegistrationNumber}");
                    return $"register number already exit {taxi.RegistrationNumber}";
                }
                if (!Enum.IsDefined(typeof(Taxi_Booking_Management.Common.Enums.TaxiStatus), taxi.TaxiStatus))
                {
                    _loggerManager.LogInfo($"Invalid task status: {taxi.TaxiStatus}");
                    return $"Invalid taxi status {taxi.TaxiStatus}";
                }
                if (taxi.TaxiType != null && !Enum.IsDefined(typeof(Taxi_Booking_Management.Common.Enums.TaxiType), taxi.TaxiType))
                {
                    _loggerManager.LogInfo($"Invalid task type: {taxi.TaxiType}");
                    return $"Invalid taxi type {taxi.TaxiType}";
                }
                await _context.taxis.AddAsync(taxi);
                await _context.SaveChangesAsync();
                _loggerManager.LogInfo($"taxi is register with taxi name: {taxi.TaxiName}");
                return "taxi is registered successfully";
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} method name : RegisterTaxiAsync");
                throw;
            }
        }
        public async Task<string> UpdateTaxiStatus(int taxiId, int taxiStatus)
        {
            try
            {
               var exTaxi = await _context.taxis.FirstOrDefaultAsync(u => u.TaxiId == taxiId);
                if(exTaxi == null)
                {
                    _loggerManager.LogInfo($"taxi not found by given taxiId {taxiId}");
                    return $"taxi not found by given taxiId {taxiId}";
                }
                if(!Enum.IsDefined(typeof(Taxi_Booking_Management.Common.Enums.TaxiStatus), taxiStatus))
                {
                    _loggerManager.LogInfo($"taxi status is invalid with given status code {taxiStatus}");
                    return "taxi status is invalid";
                }
                exTaxi.TaxiStatus = Convert.ToInt32((Taxi_Booking_Management.Common.Enums.TaxiStatus)taxiStatus);
                _context.taxis.Update(exTaxi);
                await _context.SaveChangesAsync();
                _loggerManager.LogInfo($"taxi status is successfully changed to {taxiStatus}");
                return "taxi status is successfully changed";
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} method name : UpdateTaxiStatus");
                throw;
            }
        }
    }
}