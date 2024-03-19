using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Taxi_Booking_Management.Common;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.DtoModels;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.Booking;
using Taxi_Booking_Management.Services.PaymentHistory;
using X.PagedList;

namespace Taxi_Booking_Management.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _BookingService;
    
        private readonly IPaymentHistoryService _paymentHistoryService;
        private readonly IConfiguration _configuration;
        const string bookingcontroller = "Booking";
        const string indexAction = "Index";

        public BookingController(IConfiguration configuration,IBookingService context
            , IPaymentHistoryService paymentHistoryService)
        {
            _BookingService=context;
            _paymentHistoryService = paymentHistoryService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(int? page, string search = "", int? statusFilter = 0, string? startDate = "", string? endDate = "")
        {
            ViewBag.Search = search;
            ViewBag.startDate = startDate;
            ViewBag.endDate = endDate;
            var pageNumber = page ?? 1;
            int pageSize = _configuration.GetValue<int>("AppSettings:PageSize");

            IPagedList<Booking> allBookings;
            allBookings = await _BookingService.GetAllBookingDetailsAsync(pageNumber, pageSize, search, startDate, endDate);
            if (statusFilter > 0)
            {
                if (statusFilter.HasValue)
                {
                    allBookings = allBookings.Where(t => t.BookingStatus == statusFilter).ToPagedList();
                    ViewBag.StatusFilter = statusFilter;
                }
            }
            return View(allBookings);
        }

        public async Task<IActionResult> RegisterBooking()
        {

            ViewBag.taxiName =  _BookingService.GetTaxiNames();
            ViewBag.driverName =  _BookingService.GetDriverNames();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterBooking(RegisterBookingDto dto, [FromServices] INotyfService notyf)
        {
           var data =  await _BookingService.RegisterBookingAsync(dto);
            notyf.Success(data);
            return RedirectToAction(indexAction, bookingcontroller);
        }

        [HttpGet]
        public IActionResult CheckBookingAvailbility()
        {
            CheckTaxiAvailability checkTaxi = new CheckTaxiAvailability();
            ViewBag.taxiName = _BookingService.GetTaxiNames();

            return View(checkTaxi);
        }

        [HttpPost]
        public async Task<IActionResult> CheckBookingAvailbility(CheckTaxiAvailability dto, [FromServices] INotyfService notyf)
        {
            var data = false;
            if (ModelState.IsValid)
            {
               data  = await _BookingService.IsTaxiAvailableAsync(Convert.ToInt32(dto.taxiId), dto.FromDate, dto.ToDate);
              ViewBag.TaxiDates =  await _BookingService.GetTaxiAvailableDates(Convert.ToInt32(dto.taxiId));
            }
            if (data)
            {
                notyf.Success(MessagesAlerts.TaxiIsAvailable);
            }
            else
            {
                notyf.Information(MessagesAlerts.TaxiIsNotAvailable);
            }
            return View(dto);
        }

        [HttpGet]
        public IActionResult GetAvailableTaxi()
        {
            GetAvailableTaxiDto aviTaxi = new GetAvailableTaxiDto();
            return View(aviTaxi);
        }
        [HttpPost]
        public async Task<IActionResult> GetAvailableTaxi(GetAvailableTaxiDto dto, [FromServices] INotyfService notyf)
        {
            if (ModelState.IsValid)
            {
                var data = await _BookingService.GetAvailableTaxisAsync(dto.FromDate, dto.ToDate);
                if (data.Count>0)
                {
                    notyf.Success(MessagesAlerts.TaxiIsAvailable);
                }
                else
                {
                    notyf.Information(MessagesAlerts.TaxiIsNotAvailable);
                }
                ViewBag.TaxiDetails = data;
            }
           
            return View();
        }
        


        [HttpGet]
        public async Task<IActionResult> BookingDetails(int bookingid)
        {
            Booking bookingdetails = null;
            
                bookingdetails = await _BookingService.GetTaxiBookingAsync(bookingid);
                if(bookingdetails != null)
                {
                    ViewBag.TransactionDetails = await _paymentHistoryService.GetPaymentHistoryByBookingId(bookingid);
                    var paidAmount = _paymentHistoryService.GetPaidAmountByBookingId(bookingid);
                    var dueAmount = bookingdetails.NetAmount - paidAmount;
                    ViewBag.DueAmount = dueAmount;
                    ViewBag.PaidAmount = paidAmount;
                    return View(bookingdetails);
                }
            return RedirectToAction("Error", "Home");
        }
        [HttpGet]

        public async Task<IActionResult> EditBooking(int bookingid, [FromServices] INotyfService notyf)
        {
            if (bookingid > 0)
            {
                var BookingData = await _BookingService.GetBookingDataForUpdate(bookingid);
                if (BookingData != null )
                {
                   
                    return View(BookingData);
                }
                else
                {
                    notyf.Error(MessagesAlerts.BookingDetailsNotAvailable);
                    return RedirectToAction(indexAction, bookingcontroller);
                }

            }
            notyf.Error(MessagesAlerts.InvalidId);
            return RedirectToAction(indexAction, bookingcontroller);

        }
        [HttpPost]
        public async Task<IActionResult> EditBooking(EditBookingViewModel bookingViewModel, [FromServices] INotyfService notyf)
        {
            
            if (bookingViewModel != null)
            {
                var updatedBookingData= await _BookingService.UpdateBookingAsync(bookingViewModel);

                notyf.Success($"{updatedBookingData}");
                return RedirectToAction(indexAction, bookingcontroller);
            }
            notyf.Error($"{MessagesAlerts.FailUpdate}");
            return View(bookingViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateBookingStatus(int bookingId, int bookingStatus)
        {
            if (bookingId > 0 && bookingStatus > 0)
            {
                var result = await _BookingService.UpdateBookingStatusById(bookingId, bookingStatus);
                return Json(result);
            }
            return BadRequest();

        }

        [HttpPost]
        public async Task<IActionResult> DeleteBooking(int bookingId, [FromServices] INotyfService notyf)
        {
            string message = MessagesAlerts.FailDelete;
            if (bookingId > 0)
            {
                var deleteBooking = await _BookingService.DeleteTaxiBookingsAsync(bookingId);
                if (deleteBooking.Contains("successfully"))
                {

                    notyf.Success($"{deleteBooking}");
                    return RedirectToAction(indexAction, bookingcontroller);
                }
                else
                {
                    notyf.Error($"{deleteBooking}");
                }

            }
            notyf.Error($"{message}");
            return RedirectToAction("BookingDetails", bookingcontroller);
        }
    }
}
