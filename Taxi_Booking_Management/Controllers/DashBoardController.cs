using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Services.DashBoard;

namespace Taxi_Booking_Management.Controllers
{
    [Authorize]
    public class DashBoardController : Controller
    {
        private readonly ILoggerManager _loggerManager;
        private readonly IDashBoardService _dashBoardService;

        public DashBoardController(IDashBoardService dashBoardService ,ILoggerManager loggerManager)
        {
            _loggerManager = loggerManager;
            _dashBoardService = dashBoardService;
        }

        public async Task<IActionResult> Index()
        {
            try
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

                _loggerManager.LogInfo("Dashboard data retrieved successfully");

                return View();
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while retrieving dashboard data: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
