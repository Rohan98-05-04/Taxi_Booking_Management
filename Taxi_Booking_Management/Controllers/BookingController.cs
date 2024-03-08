using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.DtoModels;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.Booking;
using X.PagedList;

namespace Taxi_Booking_Management.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _BookingService;
        private readonly ApplicationDbContext _context;

        public BookingController(IBookingService context, ApplicationDbContext dbcontext)
        {
            _BookingService=context;
            _context = dbcontext;
        }

        public async Task<IActionResult> Index(int? page, string search = "", int? statusFilter = 0)
        {
            ViewBag.Search = search;
            var pageNumber = page ?? 1;
            int pageSize = 5;

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
            return View(checkTaxi);
        }

        [HttpPost]
        public async Task<IActionResult> CheckBookingAvailbility(CheckTaxiAvailability dto, [FromServices] INotyfService notyf)
        {
            var data = false;
            if (ModelState.IsValid)
            {
                string[] taxi = dto.taxiId.Split(',');
               int exTaxi = await _BookingService.GetTaxiIdByRegNo(taxi[1]);
               data  = await _BookingService.IsTaxiAvailableAsync(exTaxi, dto.FromDate, dto.ToDate);
              ViewBag.TaxiDates=  await _BookingService.GetTaxiAvailableDates(exTaxi);
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
        

        public async Task<JsonResult> getTaxiAutoComplete(string term)
        {
            IList<string> taxies = null;
            taxies = await _BookingService.GetAllTaxiByRegNo(term);
            return Json(taxies);
        }
    }
}
