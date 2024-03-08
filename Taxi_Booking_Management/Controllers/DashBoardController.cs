using Microsoft.AspNetCore.Mvc;
using Taxi_Booking_Management.Services.DashBoard;

namespace Taxi_Booking_Management.Controllers
{
    public class DashBoardController(IDashBoardService _dashBoardService) : Controller
    {
        
        public async Task<IActionResult> Index()
        {
            int currentYear = DateTime.Now.Year;
            List<decimal> monthlyTotalBookingAmounts = _dashBoardService.GetMonthlyTotalBookingAmounts(currentYear);

            DateTime currentDate = DateTime.Now.Date;
            Dictionary<string, decimal> hourlyTotalBookingAmounts = _dashBoardService.GetDailyTotalBillAmounts(currentDate);
            ViewBag.MonthlyData = monthlyTotalBookingAmounts;
            ViewBag.DailyData = hourlyTotalBookingAmounts;
            ViewBag.TotalCustomer = await _dashBoardService.GetTotalCustomer();
            ViewBag.TotalTaxi = await _dashBoardService.GetTotalTaxiCount();
            ViewBag.TotalOwner = await _dashBoardService.GetTotalTaxiOwner();
            ViewBag.TotalDriver = await _dashBoardService.GetTotalDrivers();
            ViewBag.TotalBooking = await _dashBoardService.GetTotalBooking();
            return View();
        }
    }
}
