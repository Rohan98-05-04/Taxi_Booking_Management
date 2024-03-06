using Microsoft.AspNetCore.Mvc;
using Taxi_Booking_Management.Services.Booking;

namespace Taxi_Booking_Management.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _BookingService;

        public BookingController(IBookingService context)
        {
            _BookingService=context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
