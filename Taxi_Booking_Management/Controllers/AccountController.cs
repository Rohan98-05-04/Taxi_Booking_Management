using Microsoft.AspNetCore.Mvc;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Services.Account;
using Taxi_Booking_Management.Services.Booking;

namespace Taxi_Booking_Management.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ILoggerManager _loggerManager; // Inject ILogger

        public AccountController(IAccountService accountService, ILoggerManager loggerManage)
        {
            _accountService = accountService;
            _loggerManager = loggerManage;
        }

        [HttpGet]
        public IActionResult YearlyTotalBookingAmounts()
        {
            return View();
        }

        [HttpPost]
        public IActionResult YearlyTotalBookingAmounts(int year)
        {
            try
            {
                decimal totalAmount = _accountService.GetYearlyTotalBookingAmounts(year);
                ViewBag.Year = year;
                ViewBag.TotalAmount = totalAmount;
                _loggerManager.LogInfo($"Total booking amount for year {year} calculated successfully.");
                return View();
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while calculating total booking amount: {ex.Message}");
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View("Error");
            }
        }
        [HttpGet]
        public IActionResult YearlyTotalPaymentHistoryAmounts()
        {
            return View();
        }

        [HttpPost]
        public IActionResult YearlyTotalPaymentHistoryAmounts(int year)
        {
            try
            {
                decimal totalAmount = _accountService.GetYearlyTotalPaymentHistoryAmounts(year);
                ViewBag.Year = year;
                ViewBag.TotalAmount = totalAmount;
                _loggerManager.LogInfo($"Yearly total payment history amounts retrieved for year {year}");
                return View();
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"Error occurred while retrieving yearly total payment history amounts: {ex.Message}");
                return View("Error");
            }
        }
        [HttpGet]
        public IActionResult GetDateBillAmounts()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetDateBillAmounts(DateTime fromDate, DateTime toDate)
        {
            try
            {
                decimal totalAmount = _accountService.GetDateBillAmounts(fromDate, toDate);
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;
                ViewBag.TotalAmount = totalAmount;
                _loggerManager.LogInfo($"Bill amounts retrieved for the period from {fromDate} to {toDate}");
                return View();
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"Error occurred while retrieving bill amounts: {ex.Message}");
                return View("Error");
            }
        }
    }
}
