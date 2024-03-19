using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using Taxi_Booking_Management.Common;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.DtoModels;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.TaxiDriver;
using X.PagedList;
using static Taxi_Booking_Management.Common.Enums;

namespace Taxi_Booking_Management.Services.Booking
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerManager _loggerManager;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;

        public BookingService(ApplicationDbContext dbContext, ILoggerManager loggerManager,
              IMapper mapper, IMemoryCache memoryCache)
        {
            _context = dbContext;
            _loggerManager = loggerManager;
            _mapper = mapper;
            _memoryCache = memoryCache;
        }

        public async Task<IPagedList<Models.Booking>> GetAllBookingDetailsAsync(int page, int pageSize, string search, string? startDate, string? endDate)
        {
            IPagedList<Models.Booking> Bookings = null;
            try
            {
                IQueryable<Models.Booking> data = _context.Bookings
                .Include(t => t.taxi)
                .AsQueryable();

                if (!string.IsNullOrWhiteSpace(startDate) && DateTime.TryParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDateValue))
                {
                    data = data
                        .Where(ph => ph.FromDate >= startDateValue);
                }

                if (!string.IsNullOrWhiteSpace(endDate) && DateTime.TryParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDateValue))
                {
                    endDateValue = endDateValue.AddDays(1).Date;
                    data = data
                        .Where(ph => ph.ToDate < endDateValue);
                }

                if (!string.IsNullOrWhiteSpace(search) && data != null)
                {
                    data = data.Where(u => u.BookingCode.Contains(search) || u.CustomerName.Contains(search));

                }
                Bookings = await data.ToPagedListAsync(page, pageSize);
                _loggerManager.LogInfo($"all Bookings records are retrived");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetAllBookingDetailsAsync");
                throw;
            }
            return Bookings;

        }

        public async Task<Models.Booking> GetTaxiBookingAsync(int bookingId)
        {
            try
            {

                var retrieveBooking = await _context.Bookings.Include(t => t.taxi).Include(d => d.TaxiDrivers)
                    .FirstOrDefaultAsync(t => t.BookingId == bookingId);
                if (retrieveBooking == null)
                {
                    _loggerManager.LogInfo($"not Booking Details found with bookingId {bookingId}");
                    return null;
                }
                _loggerManager.LogInfo($"Booking details is successfully retrived with given id{bookingId}");
                return retrieveBooking;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetTaxiBookingAsync");
                throw;
            }
        }
        public async Task<EditBookingViewModel?> GetBookingDataForUpdate(int bookingid)
        {
            try
            {
                var retrieveBooking = await _context.Bookings.Include(t => t.taxi).Include(d => d.TaxiDrivers)
                    .FirstOrDefaultAsync(t => t.BookingId == bookingid);
                if (retrieveBooking != null && retrieveBooking.BookingId > 0)
                {
                    EditBookingViewModel bookingViewModeldata = new EditBookingViewModel()
                    {
                        BookingId = retrieveBooking.BookingId,
                        BookingCode = retrieveBooking.BookingCode,
                        RegistrationNo = $"{retrieveBooking.taxi.TaxiName}",
                        TaxiId = retrieveBooking.TaxiId,
                        DriverId = retrieveBooking.DriverId,
                        DriverName = _context.drivers
                                   .Select(x => new SelectListItem { Value = x.DriverId.ToString(), Text = $"{x.DriverName} ({x.DriverMobile})" }),
                        TaxiNames = _context.taxis
                                 .Select(x => new SelectListItem { Value = x.TaxiId.ToString(), Text = $"{x.TaxiName} ({x.RegistrationNumber})" }),
                        CustomerName = retrieveBooking.CustomerName,
                        CustomerMobile = retrieveBooking.CustomerMobile,
                        GrossAmount = retrieveBooking.GrossAmount,
                        TotalGST = retrieveBooking.TotalGST,
                        NetAmount = retrieveBooking.NetAmount,
                        FromLocation = retrieveBooking.FromLocation,
                        ToLocation = retrieveBooking.ToLocation,
                        BookingStatus = retrieveBooking.BookingStatus,
                        fromDate = retrieveBooking.FromDate,
                        toDate = retrieveBooking.ToDate,


                    };
                    _loggerManager.LogInfo($"Booking details is successfully retrived with given id{bookingid}");
                    return (bookingViewModeldata);
                }
                else
                {
                    _loggerManager.LogInfo($"not Booking Details found with bookingId {bookingid}");
                    return null;
                }

            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"method name: GetBookingDataForUpdate");
                throw;
            }

        }
        public async Task<string> UpdateBookingAsync(EditBookingViewModel bookingmodel)
        {
            try
            {
                bool isAvailable = true;
                var exkBookingdata = await _context.Bookings.FirstOrDefaultAsync(x => x.BookingId == bookingmodel.BookingId);
                if (exkBookingdata != null)
                {
                    if (exkBookingdata.FromDate != bookingmodel.fromDate || exkBookingdata.ToDate != bookingmodel.toDate)
                    {
                        isAvailable = await IsTaxiAvailableAsync(bookingmodel.BookingId, bookingmodel.fromDate, bookingmodel.toDate);
                    }

                    if (isAvailable)
                    {
                        exkBookingdata.FromDate = bookingmodel.fromDate;
                        exkBookingdata.ToDate = bookingmodel.toDate;
                        exkBookingdata.FromLocation = bookingmodel.FromLocation;
                        exkBookingdata.ToLocation = bookingmodel.ToLocation;
                        exkBookingdata.TaxiId = bookingmodel.TaxiId;
                        exkBookingdata.DriverId = bookingmodel.DriverId;
                        exkBookingdata.TotalGST = bookingmodel.TotalGST;
                        exkBookingdata.GrossAmount = bookingmodel.GrossAmount;
                        exkBookingdata.CustomerName = bookingmodel.CustomerName;
                        exkBookingdata.CustomerMobile = bookingmodel.CustomerMobile;
                        exkBookingdata.UpdatedDateTime = DateTime.Now;
                        decimal totalAmount = bookingmodel.GrossAmount + (bookingmodel.GrossAmount * bookingmodel.TotalGST) / 100;
                        exkBookingdata.NetAmount = totalAmount;

                        _context.Bookings.Update(exkBookingdata);
                        await _context.SaveChangesAsync();
                        _loggerManager.LogInfo($"Booking is successfully updated with given id{bookingmodel.BookingId}");
                        return $"you booking details {MessagesAlerts.SuccessfullUpdate} ";
                    }
                    else
                    {
                        _loggerManager.LogInfo($"date is not available for booking for cus :{bookingmodel.CustomerName}");
                        return $"{MessagesAlerts.TaxiIsNotAvailable}";
                    }

                }
                _loggerManager.LogInfo($"taxi not found {bookingmodel.BookingId}");
                return $"Booking Details not found  {bookingmodel.BookingCode}, {MessagesAlerts.FailUpdate}";

            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: RegisterBookingAsync");
                throw;
            }
        }
        public async Task<string> DeleteTaxiBookingsAsync(int bookingId)
        {
            string message = MessagesAlerts.FailDelete;
            try
            {
                var retriveBookingDetails = await _context.Bookings.FirstOrDefaultAsync(u => u.BookingId == bookingId);
                if (retriveBookingDetails == null)
                {
                    _loggerManager.LogInfo($"taxi Booking not found by given id{bookingId}");
                    return message;
                }
                _context.Bookings.Remove(retriveBookingDetails);
                await _context.SaveChangesAsync();
                message = MessagesAlerts.SuccessfullDelete;
                _loggerManager.LogInfo($"taxi Booking is successfully retrived with given id{bookingId}");
                return message;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: DeleteTaxiBookingsAsync");
                throw;
            }
        }

        public async Task<string> RegisterBookingAsync(RegisterBookingDto bookingDto)
        {
            try
            {
                bool isAvailable = await IsTaxiAvailableAsync(bookingDto.TaxiId, bookingDto.fromDate, bookingDto.toDate);
                if (isAvailable)
                {
                    bookingDto.BookingCode = Guid.NewGuid().ToString("N").Substring(0, 10);
                    bookingDto.UpdatedDateTime = DateTime.Now;
                    bookingDto.CreatedDateTime = DateTime.Now;
                    bookingDto.TaxiId = Convert.ToInt32(bookingDto.RegistrationNo);
                    decimal totalAmount = bookingDto.GrossAmount + (bookingDto.GrossAmount * bookingDto.TotalGST) / 100;
                    bookingDto.NetAmount = totalAmount;
                    bookingDto.BookingStatus = Convert.ToInt32(Enums.BookingStatus.Pending);

                    Models.Booking newBooking = _mapper.Map<Models.Booking>(bookingDto);
                    newBooking.DriverId = Convert.ToInt32(bookingDto.DriverName);
                    await _context.Bookings.AddAsync(newBooking);
                    await _context.SaveChangesAsync();
                    _loggerManager.LogInfo($"Booking is successfully registed with given id{bookingDto.BookingCode}");
                    return $"you booking details {MessagesAlerts.SuccessfullSave} ";
                }
                _loggerManager.LogInfo($"date is not available for booking for cus :{bookingDto.CustomerName}");
                return $"{MessagesAlerts.FailSave}";
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: RegisterBookingAsync");
                throw;
            }
        }

        public async Task<List<Models.Taxi>> GetAvailableTaxisAsync(DateTime fromDate, DateTime toDate)
        {
            var overlappingBookings = await _context.Bookings
             .Where(b =>
                 (b.FromDate >= fromDate && b.FromDate <= toDate) ||
                 (b.ToDate >= fromDate && b.ToDate <= toDate) ||
                 (b.FromDate <= fromDate && b.ToDate >= toDate))
             .Select(b => b.TaxiId)
             .Distinct()
             .ToListAsync();

            var availableTaxis = await _context.taxis
                .Where(t => !overlappingBookings.Contains(t.TaxiId) && t.TaxiStatus==1)
                .ToListAsync();

            return availableTaxis;
        }

        public async Task<bool> IsTaxiAvailableAsync(int taxiId, DateTime fromDate, DateTime toDate)
        {
            bool isTaxiAvailable = await _context.Bookings
                .AnyAsync(b => b.TaxiId == taxiId && b.FromDate <= toDate && b.ToDate >= fromDate);

            return !isTaxiAvailable;
        }




        public async Task<int> GetTaxiIdByRegNo(string regNo)
        {
            var exTaxi = await _context.taxis.FirstOrDefaultAsync(u => u.RegistrationNumber == regNo);
            if (exTaxi == null)
            {
                return 0;
            }
            return exTaxi.TaxiId;
        }

        public async Task<IList<Models.Booking>> GetTaxiAvailableDates(int taxiId)
        {
            try
            {
                DateTime todayDate = DateTime.Today;

                var availableDates = await _context.Bookings
                    .Where(b => b.TaxiId == taxiId && b.FromDate >= todayDate || b.ToDate >= todayDate)
                    .ToListAsync();
                if (availableDates.Any())
                {
                    return availableDates;
                }
                return null;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetTaxiAvailableDates");
                throw;
            }
        }

        public async Task<string> UpdateBookingStatusById(int BookingId, int BookingStatus)
        {
            try
            {
                var exBook = await _context.Bookings.FirstOrDefaultAsync(u => u.BookingId == BookingId);
                if (exBook == null)
                {
                    _loggerManager.LogInfo($"booking not found by given bookingId {BookingId}");
                    return $"booking not found by given bookingId {BookingId}";
                }
                if (!Enum.IsDefined(typeof(Taxi_Booking_Management.Common.Enums.BookingStatus), BookingStatus))
                {
                    _loggerManager.LogInfo($"booking status is invalid with given status code {BookingStatus}");
                    return "booking status is invalid";
                }
                exBook.BookingStatus = Convert.ToInt32((Taxi_Booking_Management.Common.Enums.BookingStatus)BookingStatus);
                _context.Bookings.Update(exBook);
                await _context.SaveChangesAsync();
                _loggerManager.LogInfo($"booking status is successfully changed to {BookingStatus}");
                return "booking status is successfully changed";
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} method name : UpdateBookingStatusById");
                throw;
            }
        }


        public List<SelectListItem> GetTaxiNames()
        {
            const string cacheKey = "TaxiNamesCacheKey";
            if (!_memoryCache.TryGetValue(cacheKey, out List<SelectListItem> taxiNames))
            {
                taxiNames = _context.taxis
               .Select(x => new SelectListItem { Value = x.TaxiId.ToString(), Text = $"{x.TaxiName} ({x.RegistrationNumber})" })
               .ToList();

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                };
                _memoryCache.Set(cacheKey, taxiNames, cacheEntryOptions);
            }
            return taxiNames;

        }

        public List<SelectListItem> GetDriverNames()
        {
            const string cacheKey = "TaxiDriversCacheKey";
            if (!_memoryCache.TryGetValue(cacheKey, out List<SelectListItem> taxiDrivers))
            {
                taxiDrivers = _context.drivers
                   .Select(x => new SelectListItem { Value = x.DriverId.ToString(), Text = $"{x.DriverName} ({x.DriverMobile})" }).ToList();

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                };
                _memoryCache.Set(cacheKey, taxiDrivers, cacheEntryOptions);
            }
            return taxiDrivers;

        }

       


        public async Task<Models.Booking> GetBookingByBookingCode(string bookingCode)
        {
            try
            {
                var retrieveBooking = await _context.Bookings.Include(t => t.taxi).Include(d => d.TaxiDrivers)
                    .FirstOrDefaultAsync(t => t.BookingCode == bookingCode);
                if (retrieveBooking == null)
                {
                    _loggerManager.LogInfo($"not Booking Details found with bookingCode {bookingCode}");
                    return null;
                }
                _loggerManager.LogInfo($"Booking details is successfully retrived with given booking code{bookingCode}");
                return retrieveBooking;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetBookingByBookingCode");
                throw;
            }

        }

    }
}
