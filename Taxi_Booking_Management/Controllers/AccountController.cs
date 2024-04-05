using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Taxi_Booking_Management.Common;
using Taxi_Booking_Management.Helper.PdfFormats;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Services.Account;
using Taxi_Booking_Management.Services.Booking;

namespace Taxi_Booking_Management.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ILoggerManager _loggerManager; // Inject ILogger
        private readonly IRazorViewToStringRenderer _razorViewToStringRenderer;
        private string index = "Index";
        private string dashboard = "Dashboard";

        public AccountController(IAccountService accountService, ILoggerManager loggerManage, IRazorViewToStringRenderer razorViewToStringRenderer)
        {
            _accountService = accountService;
            _loggerManager = loggerManage;
            _razorViewToStringRenderer = razorViewToStringRenderer;
        }



        [HttpGet]
        public async Task<IActionResult> YearlyTotalBookingAmounts(int year, [FromServices] INotyfService notyf)
        {
            try
            {
                if (year < 0)
                {
                    notyf.Error(MessagesAlerts.NotaYear);
                    _loggerManager.LogError($"Here Year Value is 0");
                    return RedirectToAction(index, dashboard);
                }

                if (year <= 1000 || year >= 9999)
                {
                    notyf.Error(MessagesAlerts.yearLength);
                    _loggerManager.LogError($"Here the Year input length is gretaer then 4 digit or less then 4 digit:{year} ");

                    return RedirectToAction(index, dashboard);
                }

                var AnnualAmount = _accountService.GetYearlyTotalBookingAmounts(year);
                if (AnnualAmount != null)
                {
                    if (Request.Query.ContainsKey("export") && Request.Query["export"] == "pdf")
                    {


                        // Generate HTML content for PDF (implement this method)
                        var htmlContentTask = await _accountService.CreatePdfForAnnualAmount(AnnualAmount);
                        var htmlContent = htmlContentTask;
                        // Convert HTML to PDF using DinkToPdf (implement this method)
                        var pdf = _razorViewToStringRenderer.GeneratePdf(htmlContent);
                        _loggerManager.LogInfo($"Ammount details of Specific Year PDF File download for {year} Successfully ");
                        // Set the appropriate response headers for PDF download
                        return File(pdf, "application/pdf", $"({year})AmountReport.pdf");

                    }
                }

                notyf.Error(MessagesAlerts.NotAvailableAmountDetails);
                _loggerManager.LogError($"No Amount details found with given year: {year}");
                return RedirectToAction(index, dashboard);



            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while calculating total booking amount: {ex.Message}");
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View("Error");
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetDateBillAmounts(DateTime fromDate, DateTime toDate, [FromServices] INotyfService notyf)
        {
            try
            {
                if (fromDate > toDate)
                {
                    notyf.Error("From date cannot be later than to date.");
                    _loggerManager.LogError($"From date ({fromDate.ToString("dd-MM-yyyy")}) cannot be later than to date ({toDate.ToString("dd-MM-yyyy")})");
                    return RedirectToAction(index, dashboard);
                }

                var totalAmountForDates = _accountService.GetDateBillAmounts(fromDate, toDate);
                if (totalAmountForDates != null)
                {
                    if (Request.Query.ContainsKey("export") && Request.Query["export"] == "pdf")
                    {
                        
                            // Generate HTML content for PDF (implement this method)
                            var htmlContentTask = await _accountService.CreatePdfForDateAmount(totalAmountForDates);
                            var htmlContent = htmlContentTask;
                            // Convert HTML to PDF using DinkToPdf (implement this method)
                            var pdf = _razorViewToStringRenderer.GeneratePdf(htmlContent);
                            _loggerManager.LogInfo($"Ammount details of Specific Date from ({fromDate.ToString("dd-MM-yyyy")}) to ({toDate.ToString("dd-MM-yyyy")}) PDF File download  Successfully ");
                            // Set the appropriate response headers for PDF download
                            return File(pdf, "application/pdf", $"Amount_Report_from_({fromDate.ToString("dd-MM-yyyy")})_to_({toDate.ToString("dd-MM-yyyy")}).pdf");
                        
                    }
                }

                notyf.Error(MessagesAlerts.NotAvailableAmountDetails);
                _loggerManager.LogError($"No Amount details found with given Date: from ({fromDate.ToString("dd-MM-yyyy")}) to ({toDate.ToString("dd-MM-yyyy")})");
                return RedirectToAction(index, dashboard);
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"Error occurred while retrieving bill amounts: {ex.Message}");
                return View("Error");
            }
        }
    }
}
