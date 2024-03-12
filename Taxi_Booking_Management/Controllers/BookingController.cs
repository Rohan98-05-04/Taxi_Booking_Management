using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
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
        private readonly ApplicationDbContext _context;
        private readonly IPaymentHistoryService _paymentHistoryService;

        public BookingController(IBookingService context, ApplicationDbContext dbcontext
            , IPaymentHistoryService paymentHistoryService)
        {
            _BookingService=context;
            _context = dbcontext;
            _paymentHistoryService = paymentHistoryService;
        }

        public async Task<IActionResult> Index(int? page, string search = "", int? statusFilter = 0)
        {
            ViewBag.Search = search;
            var pageNumber = page ?? 1;
            int pageSize = 10;

            IPagedList<Booking> allBookings;
            allBookings = await _BookingService.GetAllBookingDetailsAsync(pageNumber, pageSize, search);
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

        public  IActionResult RegisterBooking()
        {
            ViewBag.taxiName = _context.taxis
                                .Select(x => new SelectListItem { Value = x.TaxiId.ToString(), Text = $"{x.TaxiName} ({x.RegistrationNumber})" });
            ViewBag.driverName = _context.drivers
                                 .Select(x => new SelectListItem { Value = x.DriverId.ToString(), Text = $"{x.DriverName} ({x.DriverMobile})" });
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterBooking(RegisterBookingDto dto)
        {
           var data =  await _BookingService.RegisterBookingAsync(dto);
            return RedirectToAction("Index","Booking");
        }

        [HttpGet]
        public IActionResult CheckBookingAvailbility()
        {
            CheckTaxiAvailability checkTaxi = new CheckTaxiAvailability();
            ViewBag.taxiName = _context.taxis
               .Select(x => new SelectListItem { Value = x.TaxiId.ToString(), Text = $"{x.TaxiName} ({x.RegistrationNumber})" });
           
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
                notyf.Success("taxi is available for booking");
            }
            else
            {
                notyf.Information("taxi is not available for booking");
            }
            return View(dto);
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
                    return RedirectToAction("Index", "Booking");
                }
                else
                {
                    notyf.Error($"{deleteBooking}");
                }

            }
            notyf.Error($"{message}");
            return RedirectToAction("BookingDetails", "Booking");
        }
    }
}
