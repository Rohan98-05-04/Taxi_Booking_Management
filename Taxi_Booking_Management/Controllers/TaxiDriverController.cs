using AspNetCoreHero.ToastNotification.Abstractions;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taxi_Booking_Management.Common;
using Taxi_Booking_Management.Helper;
using Taxi_Booking_Management.Helper.PdfFormats;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.TaxiDriver;
using Taxi_Booking_Management.Services.TaxiOwner;
using X.PagedList;

namespace Taxi_Booking_Management.Controllers
{
    [Authorize]
    public class TaxiDriverController : Controller
    {
        private readonly ITaxiDriverService _taxiDriverServices;
        private readonly ILoggerManager _loggerManager;
        private readonly IConfiguration _configuration;
        private readonly IRazorViewToStringRenderer _razorViewToStringRenderer;

        public TaxiDriverController(IConfiguration configuration ,ITaxiDriverService taxiDriverServices, 
            ILoggerManager loggerManager, IRazorViewToStringRenderer razorViewToStringRenderer)
        {
            _taxiDriverServices = taxiDriverServices;
            _loggerManager = loggerManager;
            _configuration = configuration;
            _razorViewToStringRenderer = razorViewToStringRenderer;

        }



        [HttpGet]
        public async Task<IActionResult> Index(int? page, string search = "")
        {
            try
            {
                ViewBag.Search = search;
                var pageNumber = page ?? 1;
                int pageSize = _configuration.GetValue<int>("AppSettings:PageSize");

                IPagedList<TaxiDriver> allDrivers;
                allDrivers = await _taxiDriverServices.GetAllTaxiDriverAsync(pageNumber, pageSize, search);
                if (Request.Query.ContainsKey("export"))
                {
                    var exportType = Request.Query["export"];
                    if (exportType == "csv")
                    {
                        var propertiesToInclude = new string[] { "DriverName", "DriverMobile", "Address", "FilePath" };
                        var driverOwnerList = allDrivers.ToList(); 
                        var csvData = CsvExportService.GenerateCsvData(driverOwnerList, propertiesToInclude);
                        _loggerManager.LogInfo($"Successfully TaxiDriver CSV File download for {pageNumber}");
                        return File(csvData, "text/csv", "taxiDrivers.csv");
                    }
                    else if (exportType == "pdf")
                    {
                        // Generate HTML content for PDF (implement this method)
                        var htmlContentTask = _taxiDriverServices.GenerateHtmlContentForPdf(allDrivers);

                       
                        var htmlContent = await htmlContentTask;
                        var pdf = _razorViewToStringRenderer.GeneratePdf(htmlContent);
                        _loggerManager.LogInfo($"Successfully TaxiDriver PDF File download for {pageNumber}");
                        // Set the appropriate response headers for PDF download
                        return File(pdf, "application/pdf", "taxiDrivers.pdf");
                    }
                }
                return View(allDrivers);
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while fetching taxiDriver details: {ex.Message}");
                return View("Error");
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> AddDriver()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddDriver(TaxiDriver driverModel, [FromServices] INotyfService notyf)
        {
            try
            {
                string message = MessagesAlerts.FailSave;
                if (ModelState.IsValid)
                {
                    var newDriver = await _taxiDriverServices.RegisterTaxiDriverAsync(driverModel);
                    if (newDriver.Contains("successfully"))
                    {
                        notyf.Success($"{newDriver}");
                        return RedirectToAction("Index", "TaxiDriver");
                    }
                    else
                    {
                        notyf.Error($"{newDriver}");
                    }
                }
                notyf.Error($"{message}");
                return View(driverModel);
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while adding a new driver: {ex.Message}");
                return View("Error");
            }
        }

         
        [HttpGet]
        public async Task<IActionResult> DriverDetails(int driverId)
        {
            try
            {
                TaxiDriver driver = null;
                if (driverId > 0)
                {
                    driver = await _taxiDriverServices.GetTaxiDriverAsync(driverId);
                }
                return View(driver);
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while fetching driver details: {ex.Message}");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditDriver(int driverId)
        {
            TaxiDriver? driver = null;
            if (driverId > 0)
            {
                driver = await _taxiDriverServices.GetTaxiDriverAsync(driverId);

            }
            return View(driver);
        }

        [HttpPost]
        public async Task<IActionResult> EditDriver(TaxiDriver driverModel, [FromServices] INotyfService notyf)
        {
            try
            {
                string message = MessagesAlerts.FailUpdate;
                if (ModelState.IsValid)
                {
                    var updatedDriver = await _taxiDriverServices.UpdateTaxiDriverAsync(driverModel);
                    if (updatedDriver.Contains("successfully"))
                    {
                        notyf.Success($"{updatedDriver}");
                        return RedirectToAction("Index", "TaxiDriver");
                    }
                    else
                    {
                        notyf.Error($"{updatedDriver}");
                    }
                }
                notyf.Error($"{message}");
                return View(driverModel);
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while updating driver details: {ex.Message}");
                return View("Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteDriver(int driverId , [FromServices] INotyfService notyf)
        {
            try
            {
                string message = MessagesAlerts.FailDelete;
                if (driverId > 0)
                {
                    var deletedDriver = await _taxiDriverServices.DeleteTaxiDriverAsync(driverId);
                    if (deletedDriver.Contains("successfully"))
                    {
                        notyf.Success($"{deletedDriver}");
                        return RedirectToAction("Index", "TaxiDriver");
                    }
                    else
                    {
                        notyf.Error($"{deletedDriver}");
                    }
                }
                notyf.Error($"{message}");
                return RedirectToAction("DriverDetails", "TaxiDriver");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while deleting driver with ID {driverId}: {ex.Message}");
                return View("Error");
            }
        }

    }
}
