using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
                .Include(t => t.taxi ).Include(d => d.TaxiDrivers)
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
                .Where(t => !overlappingBookings.Contains(t.TaxiId) && t.TaxiStatus == 1)
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

        public string GenerateHtmlContentForPdf(IPagedList<Models.Booking> bookingData)
        {
            // Create an HTML table with student data
            var htmlBuilder = new StringBuilder();
            htmlBuilder.Append("<html><head>");
            htmlBuilder.Append("<style>");
            htmlBuilder.Append("table { border-collapse: collapse; width: 100%; border: 1px solid #000; }");
            htmlBuilder.Append("th, td { border: 1px solid #000; padding: 8px; }");
            htmlBuilder.Append("</style>");
            htmlBuilder.Append("</head><body>");
            htmlBuilder.Append("<h2>All Taxi Bookings Details</h2>");
            htmlBuilder.Append("<table>");
            htmlBuilder.Append("<thead><tr><th>Booking Code</th><th>Taxi Name</th><th>Registration Number</th><th>Customer Name</th>" +
                "<th>Customer Mobile</th><th>Driver Name</th><th>Driver Mobile</th><th>From Jounery</th><th>To Jounery</th></tr></thead>");
            htmlBuilder.Append("<tbody>");

            foreach (var items in bookingData)
            {
                htmlBuilder.Append("<tr>");
                htmlBuilder.Append($"<td class=\"text-center\">{items.BookingCode}</td>");
                htmlBuilder.Append($"<td class=\"text-center\">{items.taxi.TaxiName}</td>");
                htmlBuilder.Append($"<td class=\"text-center\">{items.taxi.RegistrationNumber}</td>");
                htmlBuilder.Append($"<td class=\"text-center\">{items.CustomerName}</td>");
                htmlBuilder.Append($"<td class=\"text-center\">{items.CustomerMobile}</td>");
                htmlBuilder.Append($"<td class=\"text-center\">{items.TaxiDrivers.DriverName}</td>");
                htmlBuilder.Append($"<td class=\"text-center\">{items.TaxiDrivers.DriverMobile}</td>");
                htmlBuilder.Append($"<td class=\"text-center\">({items.FromLocation}) ({items.FromDate.ToShortDateString()})</td>");
                htmlBuilder.Append($"<td class=\"text-center\">({items.ToLocation}) ({items.ToDate.ToShortDateString()})</td>");

                htmlBuilder.Append("</tr>");
            }

            htmlBuilder.Append("</tbody></table>");

            return htmlBuilder.ToString();


        }
        public string CreatePdfForOneBooking(Models.Booking bookingData ,IList<Models.PaymentHistory> transactionData, decimal paidAmount,decimal dueAmount  )
        {
            // Create an HTML table with student data
         
            var htmlBuilder = new StringBuilder();
            htmlBuilder.Append("<html><head>");
            htmlBuilder.Append("<link rel=\"stylesheet\" href=\"https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css\">");
            htmlBuilder.Append("<style>");
            htmlBuilder.Append("p{ font-size:20px;}");
            htmlBuilder.Append("button{ border-radius:10px;}");
            htmlBuilder.Append("</style>");
            htmlBuilder.Append("</head><body>");
            htmlBuilder.Append($"<h1>Booking Details for bookingCode( {bookingData.BookingCode})</h1>");
            htmlBuilder.Append("<div class=\"row mt-3\">");
                htmlBuilder.Append("<div class=\"col-md-6\">");
                htmlBuilder.Append($"<p><strong> Booking Status: </strong><button class=\"btn btn-primary\"> {@Enum.GetName(typeof(Taxi_Booking_Management.Common.Enums.BookingStatus), bookingData.BookingStatus)}</button></p>");
                htmlBuilder.Append($"<p class=\"text-danger\"><strong> Booking Code: </strong> {bookingData.BookingCode} </p>");
                htmlBuilder.Append($"<p><strong> Customer Name: </strong> {bookingData.CustomerName} </p>");
                htmlBuilder.Append($"<p><strong> Customer Number: </strong> {bookingData.CustomerMobile} </p>");
                htmlBuilder.Append($"<p class=\"text-danger\"><strong> Taxi Name: </strong>{ bookingData.taxi.TaxiName }</p>");
                htmlBuilder.Append($"<p><strong> Taxi Registaration Number : </strong> {bookingData.taxi.RegistrationNumber}</p>");
                htmlBuilder.Append($"<p class=\"text-danger\"><strong> Driver Name: </strong> { bookingData.TaxiDrivers.DriverName} </p>");
                htmlBuilder.Append($"<p class=\"text-danger\"><strong> Driver Name: </strong> { bookingData.TaxiDrivers.DriverMobile} </p>");
                htmlBuilder.Append("</div>");
                 htmlBuilder.Append("<div class=\"col-md-6\">");
                 htmlBuilder.Append($"<p><strong> From Date: </strong> {bookingData.FromDate} </p>");
                htmlBuilder.Append($"<p><strong> To Date: </strong> {bookingData.ToDate} </p>");
                htmlBuilder.Append($"<p class=\"text-danger\"><strong> Journey Start: </strong> { bookingData.FromLocation} </p>");
                htmlBuilder.Append($"<p class=\"text-danger\"><strong> Journey End: </strong> { bookingData.ToLocation }</p>");
                htmlBuilder.Append($"<p><strong> Gross Amount: </strong>{bookingData.GrossAmount} </p>");
                htmlBuilder.Append($"<p><strong> Gst(%) : </strong> {bookingData.TotalGST} </p>");
                htmlBuilder.Append($"<p><strong> Net Amount: </strong> {bookingData.NetAmount} </p>");
                
                htmlBuilder.Append("</div>");
                htmlBuilder.Append("</div>");

            htmlBuilder.Append("<hr class=\"my-4\" style=\"border-color: red;\" />");
            htmlBuilder.Append("<div>");
            htmlBuilder.Append("<div class=\"d-flex justify-content-between align-items-center\">");
            htmlBuilder.Append("<h1 class=\"mt-4 mb-3 text-warning fw-bolder\">Payment Details</h1>");

            if (paidAmount >= bookingData.NetAmount)
            {

                htmlBuilder.Append("<h3 class=\"mt-4 mb-3 text-danger fw-bold\">Payment Status:- ");
                htmlBuilder.Append("<button class=\"btn btn-success\">");
                htmlBuilder.Append(Enum.GetName(typeof(Taxi_Booking_Management.Common.Enums.BookingPaymentStatus), 2));
                htmlBuilder.Append("</button>");

            }
            else
            {
                htmlBuilder.Append("<h3 class=\"mt-4 mb-3 text-danger fw-bold\">Payment Status:- ");
                htmlBuilder.Append("<button class=\"btn btn-warning\">");
                htmlBuilder.Append(Enum.GetName(typeof(Taxi_Booking_Management.Common.Enums.BookingPaymentStatus), 1));
                htmlBuilder.Append("</button>");
                htmlBuilder.Append("</h3>");
            }

            htmlBuilder.Append("</div>");
            htmlBuilder.Append("<div>");
            htmlBuilder.Append("<h3 class=\"mt-4 mb-3 text-danger fw-bold\">Total due amount: ");
            //htmlBuilder.Append(_configuration["AppSettings:indianCurrency"]);
            htmlBuilder.Append(dueAmount);
            htmlBuilder.Append("</h3>");
            htmlBuilder.Append("<h3 class=\"mt-4 mb-3 text-danger fw-bold\">Total paid amount: ");
            //htmlBuilder.Append(_configuration["AppSettings:indianCurrency"]);
            htmlBuilder.Append(paidAmount);
            htmlBuilder.Append("</h3>");
            htmlBuilder.Append("</div>");
            htmlBuilder.Append("</div>");

            htmlBuilder.Append("<div class=\"list-group\">");
            foreach (var transaction in transactionData)
            {
                htmlBuilder.Append("<div class=\"list-group-item border-danger\">");
                htmlBuilder.Append("<div class=\"d-flex w-100 justify-content-between\">");
                htmlBuilder.Append("<h4 class=\"mb-1\">Payment Amount: ");
                //htmlBuilder.Append();
                htmlBuilder.Append(transaction.PayAmount);
                htmlBuilder.Append(" (");
                htmlBuilder.Append(Enum.GetName(typeof(Taxi_Booking_Management.Common.Enums.PaymentMedium), transaction.PaidMedium));
                htmlBuilder.Append(")</h4>");
                htmlBuilder.Append("<small>Date: ");
                htmlBuilder.Append(transaction.CreateDateTime);
                htmlBuilder.Append("</small>");
                htmlBuilder.Append("</div>");
                htmlBuilder.Append("</div>");
                htmlBuilder.Append("<hr class=\"my-2\" style=\"border-color: yellow;\" />");
            }
            htmlBuilder.Append("</div>");


            htmlBuilder.Append("</body></html>");

            return htmlBuilder.ToString();

        }
    }
}


